﻿namespace Microsoft.VisualStudio.Composition.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Composition.Hosting;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    using MefV1 = System.ComponentModel.Composition;
    using CompositionFailedException = Microsoft.VisualStudio.Composition.CompositionFailedException;

    public class ExportingMembersTests
    {
        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass))]
        public void ExportedField(IContainer container)
        {
            string actual = container.GetExportedValue<string>("Field");
            Assert.Equal("Andrew", actual);

            actual = container.GetExportedValue<string>("Field_Extra_Export");
            Assert.Equal("Andrew", actual);
        }

        [MefFact(CompositionEngines.V1Compat | CompositionEngines.V2Compat, typeof(ExportingMembersClass))]
        public void ExportedProperty(IContainer container)
        {
            string actual = container.GetExportedValue<string>("Property");
            Assert.Equal("Andrew", actual);

            actual = container.GetExportedValue<string>("Property_Extra_Export");
            Assert.Equal("Andrew", actual);
        }

        [MefFact(CompositionEngines.V1Compat | CompositionEngines.V2Compat, typeof(ExportingMembersClass))]
        [Trait("Container.GetExport", "Plural")]
        public void GetExportsFromProperty(IContainer container)
        {
            IEnumerable<string> result = container.GetExportedValues<string>("Property");
            Assert.Equal(new[] { "Andrew" }, result);
        }

        [MefFact(CompositionEngines.V1Compat | CompositionEngines.V2Compat, typeof(ExportingMembersClass))]
        [Trait("GenericExports", "Closed")]
        public void ExportedPropertyGenericType(IContainer container)
        {
            var actual = container.GetExportedValue<Comparer<int>>("PropertyGenericType");
            Assert.NotNull(actual);
        }

        [MefFact(CompositionEngines.V1Compat | CompositionEngines.V2Compat, typeof(ExportingMembersClass))]
        [Trait("GenericExports", "Closed")]
        public void ExportedPropertyGenericTypeWrongTypeArgs(IContainer container)
        {
            Assert.Throws<CompositionFailedException>(() => container.GetExportedValue<Comparer<bool>>("PropertyGenericType"));
        }

        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass))]
        public void ExportedMethodAction(IContainer container)
        {
            var actual = container.GetExportedValue<Action>("Method");
            Assert.NotNull(actual);

            actual = container.GetExportedValue<Action>("Method_Extra_Export");
            Assert.NotNull(actual);
        }

        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass))]
        [Trait("GenericExports", "Closed")]
        public void ExportedMethodActionOf2(IContainer container)
        {
            var actual = container.GetExportedValue<Action<int, string>>("Method");
            Assert.NotNull(actual);
        }

        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass))]
        [Trait("GenericExports", "Closed")]
        public void ExportedMethodFunc(IContainer container)
        {
            var actual = container.GetExportedValue<Func<bool>>("Method");
            Assert.NotNull(actual);
        }

        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass))]
        [Trait("GenericExports", "Closed")]
        public void ExportedMethodFuncOf2(IContainer container)
        {
            var actual = container.GetExportedValue<Func<int, string, bool>>("Method");
            Assert.NotNull(actual);
        }

        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass))]
        [Trait("GenericExports", "Closed")]
        public void ExportedMethodFuncOf2WrongTypeArgs(IContainer container)
        {
            try
            {
                container.GetExportedValue<Func<string, string, bool>>("Method");
                Assert.False(true, "Expected exception not thrown.");
            }
            catch (MefV1.ImportCardinalityMismatchException) { }
            catch (Microsoft.VisualStudio.Composition.CompositionFailedException) { } // V2/V3
        }

        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass), typeof(ImportingClass))]
        [Trait("GenericExports", "Closed")]
        public void ImportOfExportedMethodFuncOf2(IContainer container)
        {
            var importer = container.GetExportedValue<ImportingClass>();
            Assert.NotNull(importer.FuncOf2);
        }

        [MefFact(CompositionEngines.V1Compat, typeof(ExportingMembersClass), typeof(ImportingClass))]
        [Trait("GenericExports", "Closed")]
        public void ImportOfExportedMethodFuncOf2WrongTypeArgs(IContainer container)
        {
            var importer = container.GetExportedValue<ImportingClass>();
            Assert.Null(importer.FuncOf2WrongTypeArgs);
        }

        [MefV1.Export]
        public class ImportingClass
        {
            [MefV1.Import("Method")]
            public Func<int, string, bool> FuncOf2 { get; set; }

            [MefV1.Import("Method", AllowDefault = true)]
            public Func<string, string, bool> FuncOf2WrongTypeArgs { get; set; }
        }

        public class ExportingMembersClass
        {
            [MefV1.Export("Field")]
            [MefV1.Export("Field_Extra_Export")]
            public string Field = "Andrew";

            [Export("Property")]
            [MefV1.Export("Property")]
            [Export("Property_Extra_Export")]
            [MefV1.Export("Property_Extra_Export")]
            public string Property
            {
                get { return "Andrew"; }
            }

            [Export("PropertyGenericType")]
            [MefV1.Export("PropertyGenericType")]
            public Comparer<int> PropertyGenericType
            {
                get { return Comparer<int>.Default; }
            }

            [MefV1.Export("Method")]
            [MefV1.Export("Method_Extra_Export")]
            public void SomeAction()
            {
            }

            [MefV1.Export("Method")]
            public void SomeAction(int a, string b)
            {
            }

            [MefV1.Export("Method")]
            public bool SomeFunc()
            {
                return true;
            }

            [MefV1.Export("Method")]
            public bool SomeFunc(int a, string b)
            {
                return true;
            }
        }

        #region Custom delegate type tests

        [MefFact(CompositionEngines.V1, typeof(PartImportingDelegatesOfCustomType), typeof(PartExportingDelegateOfCustomType))]
        public void EventHandlerAsExports(IContainer container)
        {
            var importer = container.GetExportedValue<PartImportingDelegatesOfCustomType>();
            Assert.Equal(1, importer.Handlers.Count);
            importer.Handlers[0](null, null);
            Assert.Equal(1, container.GetExportedValue<PartExportingDelegateOfCustomType>().InvocationCount);
        }

        [MefFact(CompositionEngines.V1, typeof(PartImportingDelegatesOfCustomType), typeof(PartExportingIncompatibleDelegateType), InvalidConfiguration = true)]
        public void IncompatibleDelegateExported(IContainer container)
        {
            container.GetExportedValue<PartImportingDelegatesOfCustomType>();
        }

        [MefV1.Export]
        public class PartImportingDelegatesOfCustomType
        {
            [MefV1.ImportMany]
            public List<EventHandler> Handlers { get; set; }
        }

        [MefV1.Export]
        public class PartExportingDelegateOfCustomType
        {
            public int InvocationCount { get; set; }

            [MefV1.Export(typeof(EventHandler))]
            public void Handler(object sender, EventArgs e)
            {
                this.InvocationCount++;
            }
        }

        public class PartExportingIncompatibleDelegateType
        {
            [MefV1.Export(typeof(EventHandler))]
            public void Handler()
            {
            }
        }

        #endregion
    }
}
