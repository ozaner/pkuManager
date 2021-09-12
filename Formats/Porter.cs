using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats
{
    public abstract class Porter
    {
        /// <summary>
        /// The pku file being ported.
        /// </summary>
        protected pkuObject pku { get; set; }

        /// <summary>
        /// The global flags to, optionally, be acted upon by the porter.
        /// </summary>
        protected GlobalFlags GlobalFlags { get; }

        /// <summary>
        /// The type of the <see cref="Data"/> that this porter makes use of.<br/>
        /// Type must be a child of <see cref="FormatObject"/>.
        /// </summary>
        protected abstract Type DataType { get; }

        /// <summary>
        /// A data structure representing the non-pku format.
        /// </summary>
        protected FormatObject Data { get; }

        /// <summary>
        /// A list of notes to be displayed on the porter window.<br/>
        /// A note is an alert that, generally, regards some pre-processing directive.
        /// </summary>
        public List<Alert> Notes { get; } = new();

        /// <summary>
        /// Base porter constructor.
        /// </summary>
        /// <param name="pku">The pku to be ported.</param>
        /// <param name="globalFlags">The current collection's flag settings.</param>
        public Porter(pkuObject pku, GlobalFlags globalFlags)
        {
            this.pku = pku ?? throw new ArgumentException("Can't initialize an exporter with a null .pku!");
            GlobalFlags = globalFlags;
            Data = (FormatObject)Activator.CreateInstance(DataType);
        }

        /// <summary>
        /// Determines whether or not the given file can be ported to the desired format, and a reason why if it cannot.
        /// </summary>
        /// <returns>A 2-tuple of a bool of whether the port is possible and a string with a reason why if it can't.<br/>
        ///          The string is <see langword="null"/> if a) is <see langword="true"/>.</returns>
        public abstract (bool canPort, string reason) CanPort();

        /// <summary>
        /// Searches for all members in this instance's class with the <see cref="PorterDirective"/> attribute.
        /// </summary>
        /// <param name="phase">The <see cref="ProcessingPhase"/> of the members to be searched for.</param>
        /// <returns>A list of MemberInfo objects of members with the <see cref="PorterDirective"/> attribute.</returns>
        private List<MemberInfo> GetExporterDirectiveMembers(ProcessingPhase phase)
        {
            return GetType().GetMembers(BindingFlags.Instance | BindingFlags.NonPublic).Where(m =>
                (m.GetCustomAttribute(typeof(PorterDirective), true) as PorterDirective)?.Phase == phase).ToList();
        }

        /// <summary>
        /// Generates an exception to be thrown when an <see cref="PorterDirective"/> attribute is placed on an invalid member.
        /// </summary>
        /// <returns>An exception for members with invalid <see cref="PorterDirective"/> attributes.</returns>
        private static Exception InvalidAttributeException()
        {
            return new Exception($"{nameof(PorterDirective)} attributes can only be placed on void methods" +
                $" with no parameters and properties of type {typeof(ErrorResolver<>).Name}.");
        }

        /// <summary>
        /// Given a list of <paramref name="members"/>, invokes void methods and
        /// decides values for <see cref="ErrorResolver{T}"/>s respectively.
        /// </summary>
        /// <param name="members">A list of MemberInfo objects. Should not include any members but
        ///                       <see cref="ErrorResolver{T}"/>, and void parameterless methods.</param>
        private void RunMembers(List<MemberInfo> members)
        {
            void RunMember(MemberInfo member, List<MemberInfo> members)
            {
                if (member is null) //already consumed/dne
                    return;

                PorterDirective ed = member.GetCustomAttribute(typeof(PorterDirective), true) as PorterDirective;
                if (ed.Prerequisites?.Length > 0) //has prereqs
                {
                    foreach (string prereq in ed.Prerequisites)
                        RunMember(members.FirstOrDefault(x => x.Name == prereq), members);
                }

                if (member.MemberType is MemberTypes.Method)
                {
                    MethodInfo minfo = member as MethodInfo;
                    if (minfo.ReturnType != typeof(void) && minfo.GetParameters() is not null)
                        throw InvalidAttributeException();
                    minfo.Invoke(this, null);
                }
                else if (member.MemberType is MemberTypes.Property)
                {
                    var resolver = (member as PropertyInfo).GetValue(this);
                    if (resolver.GetType().GetGenericTypeDefinition() != typeof(ErrorResolver<>))
                        throw InvalidAttributeException();
                    resolver.GetType().GetMethod(nameof(ErrorResolver<object>.DecideValue)).Invoke(resolver, null);
                }
                members.Remove(member); //consumed
            }

            while (members.Any())
                RunMember(members.First(), members);
        }

        /// <summary>
        /// Whether or not <see cref="FirstHalf"/> was run yet.
        /// </summary>
        private bool firstHalf = false;

        /// <summary>
        /// The first half of the exporting process. Runs the <see cref="ProcessingPhase.PreProcessing"/>
        /// and <see cref="ProcessingPhase.FirstPass"/> phases.<br/>
        /// Should only be run if <see cref="CanPort"/> returns true.
        /// </summary>
        public void FirstHalf()
        {
            if (!CanPort().canPort)
                throw new Exception("This .pku can't be exported to this format! This should not have happened...");

            var members = GetExporterDirectiveMembers(ProcessingPhase.PreProcessing);
            RunMembers(members);
            members = GetExporterDirectiveMembers(ProcessingPhase.FirstPass);
            RunMembers(members);

            firstHalf = true;
        }

        /// <summary>
        /// The second half of the exporting process. Runs the <see cref="ProcessingPhase.SecondPass"/> phase.
        /// Should only be run after <see cref="FirstHalf"/>.
        /// </summary>
        protected void SecondHalf()
        {
            if (!firstHalf)
                throw new Exception($"{nameof(FirstHalf)} has not been run yet! This should not have happened...");

            var members = GetExporterDirectiveMembers(ProcessingPhase.SecondPass);
            RunMembers(members);
        }
    }

    /// <summary>
    /// An attribute to be placed on methods or <see cref="ErrorResolver{T}"/>s that
    /// defines a <see cref=Porter"/>s order of operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class PorterDirective : Attribute
    {
        /// <summary>
        /// The phase this member should be run in during the porting process.
        /// </summary>
        public ProcessingPhase Phase { get; }

        /// <summary>
        /// A list of members, in the same phase, that are to be run before this one.
        /// </summary>
        public string[] Prerequisites { get; }

        /// <summary>
        /// Creates an PorterDirective in the given <paramref name="phase"/> with the given <paramref name="prerequisites"/>.
        /// </summary>
        /// <param name="phase">The phase this member will run in.</param>
        /// <param name="prerequisites">A list of members, in the same phase, that are to be run before this one.</param>
        public PorterDirective(ProcessingPhase phase, params string[] prerequisites)
        {
            Phase = phase;
            Prerequisites = prerequisites;
        }

        /// <summary>
        /// A phase of the porting process.
        /// </summary>
        public enum ProcessingPhase
        {
            /// <summary>
            /// Reserved for applying the format override, if it exists. Should occur before all other stages.
            /// </summary>
            FormatOverride,

            /// <summary>
            /// Usually for dealing with <see cref="GlobalFlags"/> before porting.
            /// </summary>
            PreProcessing,

            /// <summary>
            /// For generating the values and alerts that may need to be displayed to the user.
            /// </summary>
            FirstPass,

            /// <summary>
            /// For deciding on which values to use in the final file, based on user input.
            /// </summary>
            SecondPass
        };
    }
}
