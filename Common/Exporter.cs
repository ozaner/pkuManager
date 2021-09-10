using pkuManager.Alerts;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using static pkuManager.Common.ExporterDirective;

namespace pkuManager.Common
{
    /// <summary>
    /// The base class for all Exporters, all formats that can be<br/>
    /// exported must have a corresponding class implements this.
    /// </summary>
    public abstract class Exporter
    {
        /// <summary>
        /// The particular pku to be exported.
        /// </summary>
        protected pkuObject pku { get; init; }

        /// <summary>
        /// The global flags to, optionally, be acted upon by the exporter.
        /// </summary>
        protected GlobalFlags GlobalFlags { get; init; }

        /// <summary>
        /// A data structure representing the exported file.
        /// </summary>
        protected FormatObject Data { get; init; }

        /// <summary>
        /// A list of warnings to be displayed on the exporter window.<br/>
        /// A warning is an alert about a value that, generally, requires no input from the user.
        /// </summary>
        public List<Alert> Warnings{ get; init; }

        /// <summary>
        /// A list of errors to be displayed on the exporter window.<br/>
        /// An error is an alert about a value that, generally, requires input from the user.
        /// </summary>
        public List<Alert> Errors { get; init; }

        /// <summary>
        /// A list of notes to be displayed on the exporter window.<br/>
        /// A note is an alert that, generally, regards some pre-processing directive.
        /// </summary>
        public List<Alert> Notes { get; init; }

        /// <summary>
        /// Base exporter constructor.
        /// </summary>
        /// <param name="pku">The pku to be exported.</param>
        /// <param name="globalFlags">The current collection's flag settings.</param>
        /// <param name="data">A data structure representing the intended format.<br/>
        ///                    This parameter should be hidden by any implementation of this class.</param>
        public Exporter(pkuObject pku, GlobalFlags globalFlags, FormatObject data)
        {
            this.pku = pku ?? throw new ArgumentException("Can't make an exporter with a null .pku!");
            GlobalFlags = globalFlags;
            Data = data;
            Warnings = new List<Alert>();
            Errors = new List<Alert>();
            Notes = new List<Alert>();
        }

        /// <summary>
        /// Whether or not the passed <see cref="pku"/> can be exported to this format at all.
        /// </summary>
        /// <returns>A bool denoting if the <see cref="pku"/> can be exported.</returns>
        public abstract bool CanExport();

        /// <summary>
        /// Searches for all members in this instance's class with the <see cref="ExporterDirective"/> attribute.
        /// </summary>
        /// <param name="phase">The <see cref="ProcessingPhase"/> of the members to be searched for.</param>
        /// <returns>A list of MemberInfo objects of members with the <see cref="ExporterDirective"/> attribute.</returns>
        private List<MemberInfo> GetExporterDirectiveMembers(ProcessingPhase phase)
        {
            return GetType().GetMembers(BindingFlags.Instance | BindingFlags.NonPublic).Where(m =>
                (m.GetCustomAttribute(typeof(ExporterDirective), true) as ExporterDirective)?.Phase == phase).ToList();
        }

        /// <summary>
        /// Generates an exception to be thrown when an <see cref="ExporterDirective"/> attribute is placed on an invalid member.
        /// </summary>
        /// <returns>An exception for members with invalid <see cref="ExporterDirective"/> attributes.</returns>
        private static Exception InvalidAttributeException()
        {
            return new Exception($"{nameof(ExporterDirective)} attributes can only be placed on void methods with no parameters and properties of type {typeof(ErrorResolver<>).Name}.");
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

                ExporterDirective ed = member.GetCustomAttribute(typeof(ExporterDirective), true) as ExporterDirective;
                if (ed.Prerequisites?.Length > 0) //has prereqs
                {
                    foreach (string prereq in ed.Prerequisites)
                        RunMember(members.FirstOrDefault(x => x.Name == prereq), members);
                }

                if (member.MemberType is MemberTypes.Method)
                {
                    MethodInfo minfo = (member as MethodInfo);
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
        /// Whether or not <see cref="BeforeToFile"/> was run yet.
        /// </summary>
        private bool beforeToFile = false;

        /// <summary>
        /// The first half of the exporting process. Runs the <see cref="ProcessingPhase.PreProcessing"/>
        /// and <see cref="ProcessingPhase.FirstPass"/> phases.<br/>
        /// Should only be run if <see cref="CanExport"/> returns true.
        /// </summary>
        public void BeforeToFile()
        {
            if (!CanExport())
                throw new Exception("This .pku can't be exported to this format! This should not have happened...");

            var members = GetExporterDirectiveMembers(ProcessingPhase.PreProcessing);
            RunMembers(members);
            members = GetExporterDirectiveMembers(ProcessingPhase.FirstPass);
            RunMembers(members);

            beforeToFile = true;
        }

        /// <summary>
        /// The second half of the exporting process. Runs the <see cref="ProcessingPhase.SecondPass"/><br/>
        /// phase and returns the exported file generated from the given <see cref="pku"/>.<br/>
        /// Should only be run after <see cref="BeforeToFile"/>.
        /// </summary>
        /// <returns>A <see cref="byte"/> array representation of the exported file.</returns>
        public byte[] ToFile()
        {
            if (!beforeToFile)
                throw new Exception($"{nameof(BeforeToFile)} has not been run yet! This should not have happened...");

            var members = GetExporterDirectiveMembers(ProcessingPhase.SecondPass);
            RunMembers(members);

            return Data.ToFile();
        }
    }

    /// <summary>
    /// An attribute to be placed on methods or <see cref="ErrorResolver{T}"/>s that
    /// define the order of operations in <see cref="Exporter"/>s.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ExporterDirective : Attribute
    {
        /// <summary>
        /// The phase this member should be run in during the exporting process.
        /// </summary>
        public ProcessingPhase Phase { get; init; }

        /// <summary>
        /// A list of members, in the same phase, that are to be run before this one.
        /// </summary>
        public string[] Prerequisites { get; init; }

        /// <summary>
        /// Creates an ExporterDirective in the given <paramref name="phase"/> with the given <paramref name="prerequisites"/>.
        /// </summary>
        /// <param name="phase">The phase this member will run in.</param>
        /// <param name="prerequisites">A list of members, in the same phase, that are to be run before this one.</param>
        public ExporterDirective(ProcessingPhase phase, params string[] prerequisites)
        {
            Phase = phase;
            Prerequisites = prerequisites;
        }

        /// <summary>
        /// A phase of the exporting process.
        /// </summary>
        public enum ProcessingPhase
        {
            /// <summary>
            /// For modifications to the <see cref="pkuObject"/> itself, before exporting.
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
