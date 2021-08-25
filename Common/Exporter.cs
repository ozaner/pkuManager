using pkuManager.Alerts;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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
        /// <param name="secondPass">If true, only members marked for the second pass will be returned.<br/>
        ///                          If false, members marked for the first pass are returned.</param>
        /// <returns>An ordered list of MemberInfo objects of members with the <see cref="ExporterDirective"/> attribute.</returns>
        private IOrderedEnumerable<MemberInfo> GetExporterDirectiveMembers(bool secondPass)
        {
            return GetType().GetMembers(BindingFlags.Instance | BindingFlags.NonPublic).Where(m =>
                (m.GetCustomAttribute(typeof(ExporterDirective), true) as ExporterDirective)?.SecondPass == secondPass
            ).OrderBy(m => (m.GetCustomAttribute(typeof(ExporterDirective), true) as ExporterDirective).Order);
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
        /// decides values for <see cref="ErrorResolver{T}"/> respectively.
        /// </summary>
        /// <param name="members">A list of MemberInfo objects. Should not include any members but
        ///                       <see cref="ErrorResolver{T}"/>, and void parameterless methods.</param>
        private void RunMembers(IOrderedEnumerable<MemberInfo> members)
        {
            foreach (MemberInfo member in members)
            {
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
            }
        }

        /// <summary>
        /// Whether or not <see cref="FirstPass"/> was run yet.
        /// </summary>
        private bool firstPass = false;

        /// <summary>
        /// The first pass of the exporting process. All alerts to be shown the user should be generated here.<br/>
        /// Should only be run if <see cref="CanExport"/> returns true.
        /// </summary>
        public void FirstPass()
        {
            if (!CanExport())
                throw new Exception("This .pku can't be exported to this format! This should not have happened.");
            var members = GetExporterDirectiveMembers(false);
            RunMembers(members);
            firstPass = true;
        }

        /// <summary>
        /// The second pass of the exporting process.
        /// This should be run once the user has decided what options to pick in the error resolution step, if any.
        /// </summary>
        private void SecondPass()
        {
            if (!firstPass)
                throw new Exception("The first pass for this .pku has not occured yet! This should not have happened.");
            var members = GetExporterDirectiveMembers(true);
            RunMembers(members);
        }

        /// <summary>
        /// Returns the exported file generated from the given <see cref="pku"/>.<br/>
        /// Should only be run after <see cref="FirstPass"/>.
        /// </summary>
        /// <returns>A <see cref="byte"/> array representation of the exported file.</returns>
        public byte[] ToFile()
        {
            SecondPass();
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
        /// The priority of this member to be run when exporting.
        /// </summary>
        public int Order { get; init; }

        /// <summary>
        /// Whether this member is to be run in the second phase or not (i.e. first phase).
        /// </summary>
        public bool SecondPass { get; init; }

        /// <summary>
        /// Creates an ExporterDirective with the given priority (<paramref name="order"/>)
        /// and phase (<paramref name="secondPass"/>).
        /// </summary>
        /// <param name="order">The priority of this member to be run when exporting.</param>
        /// <param name="secondPass">Whether this member is to be run in the second phase or not (i.e. first phase).</param>
        public ExporterDirective(int order, bool secondPass = false)
        {
            Order = order;
            SecondPass = secondPass;
        }
    }
}
