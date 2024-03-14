using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Gorgon.Core.Test.Support;
using Gorgon.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gorgon.Core.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ReflectionExtensionTests
    {
        [TestMethod]
        public void PropertyObjectTypeConversion()
        {
            var inst = new PropertyTests();
            Type instType = inst.GetType();
            PropertyInfo propRO = instType.GetProperty("MyReadOnlyProperty");
            PropertyInfo propRW = instType.GetProperty("MyProperty");

            Assert.IsNotNull(propRO);

            inst.MyWriteOnlyProperty = 999;

            PropertyGetter<PropertyTests, object> getter = propRO.CreatePropertyGetter<PropertyTests, object>();

            Assert.IsNotNull(getter);

            object actual = getter(inst);

            Assert.IsNotNull(actual);

            Assert.AreEqual("999", actual);

            PropertySetter<PropertyTests, object> setter = propRW.CreatePropertySetter<PropertyTests, object>();

            Assert.IsNotNull(setter);

            setter(inst, 123);

            Assert.AreEqual(123, inst.MyProperty);
        }

        [TestMethod]
        public void ReadOnlyProperty()
        {
            var inst = new PropertyTests();
            Type instType = inst.GetType();
            PropertyInfo propRO = instType.GetProperty("MyReadOnlyProperty");

            Assert.IsNotNull(propRO);

            inst.MyWriteOnlyProperty = 999;

            PropertyGetter<PropertyTests, string> getter = propRO.CreatePropertyGetter<PropertyTests, string>();

            Assert.IsNotNull(getter);

            string actual = getter(inst);

            Assert.AreEqual("999", actual);

            try
            {
                propRO.CreatePropertySetter<PropertyTests, string>();
                Assert.Fail("Setter should not exist.");
            }
            catch (ArgumentException)
            {

            }

            propRO = instType.GetProperty("MyReadOnlyAutoProperty");

            Assert.IsNotNull(propRO);

            getter = propRO.CreatePropertyGetter<PropertyTests, string>();

            Assert.IsNotNull(getter);

            Assert.AreEqual("MyAuto", getter(inst));

            PropertySetter<PropertyTests, string> setter = propRO.CreatePropertySetter<PropertyTests, string>();

            Assert.IsNotNull(setter);

            setter(inst, "NoAuto");

            Assert.AreEqual("NoAuto", inst.MyReadOnlyAutoProperty);
        }

        [TestMethod]
        public void WriteOnlyProperty()
        {
            var inst = new PropertyTests();
            Type instType = inst.GetType();
            PropertyInfo propWO = instType.GetProperty("MyWriteOnlyProperty");

            Assert.IsNotNull(propWO);

            PropertySetter<PropertyTests, int> setter = propWO.CreatePropertySetter<PropertyTests, int>();

            Assert.IsNotNull(setter);

            setter(inst, 789);

            Assert.AreEqual("789", inst.MyReadOnlyProperty);

            PropertyGetter<PropertyTests, int> getter = propWO.CreatePropertyGetter<PropertyTests, int>();

            Assert.IsNotNull(getter);

            int actual = Convert.ToInt32(getter(inst));

            Assert.AreEqual(789, actual);
        }

        [TestMethod]
        public void ReadWriteProperty()
        {
            var inst = new PropertyTests();
            Type instType = inst.GetType();
            PropertyInfo propRW = instType.GetProperty("MyProperty");

            Assert.IsNotNull(propRW);

            PropertyGetter<PropertyTests, int> getter = propRW.CreatePropertyGetter<PropertyTests, int>();
            PropertySetter<PropertyTests, int> setter = propRW.CreatePropertySetter<PropertyTests, int>();

            Assert.IsNotNull(getter);
            Assert.IsNotNull(setter);

            setter(inst, 456);

            Assert.AreEqual(456, inst.MyProperty);

            int actual = Convert.ToInt32(getter(inst));

            Assert.AreEqual(456, actual);
        }

        [TestMethod]
        public void CreatePublicObject()
        {
            ActivatorTestClass obj;
            Type type = typeof(ActivatorTestClass);
            ObjectActivator<ActivatorTestClass> activator = type.CreateActivator<ActivatorTestClass>();

            obj = activator();

            Assert.IsNotNull(obj);

            Assert.AreEqual(111, obj.TestThis());
        }

        [TestMethod]
        public void CreateInternalObject()
        {
            InternalActivatorTestClass obj;
            Type type = typeof(InternalActivatorTestClass);
            ObjectActivator<InternalActivatorTestClass> activator = type.CreateActivator<InternalActivatorTestClass>();

            obj = activator();

            Assert.IsNotNull(obj);

            Assert.AreEqual(123, obj.TestThis());
        }

        [TestMethod]
        public void CreateMultipleParamsObject()
        {
            ActivatorTestClassMultipleParams obj;
            Type type = typeof(ActivatorTestClassMultipleParams);
            ObjectActivator<ActivatorTestClassMultipleParams> activator = type.CreateActivator<ActivatorTestClassMultipleParams>(typeof(int), typeof(int));

            obj = activator(123, 123);

            Assert.IsNotNull(obj);

            Assert.AreEqual(246, obj.TestThis());
        }

        [TestMethod]
        public void CreateMultipleCtor()
        {
            ActivatorTestClassMultipleCtors obj;
            Type type = typeof(ActivatorTestClassMultipleCtors);
            ObjectActivator<ActivatorTestClassMultipleCtors> activator1 = type.CreateActivator<ActivatorTestClassMultipleCtors>(typeof(int), typeof(int));
            ObjectActivator<ActivatorTestClassMultipleCtors> activator2 = type.CreateActivator<ActivatorTestClassMultipleCtors>(typeof(int));
            ObjectActivator<ActivatorTestClassMultipleCtors> activator3 = type.CreateActivator<ActivatorTestClassMultipleCtors>();

            obj = activator1(123, 123);

            Assert.IsNotNull(obj);

            Assert.AreEqual(246, obj.TestThis());

            obj = activator2(123);

            Assert.IsNotNull(obj);

            Assert.AreEqual(246, obj.TestThis());

            obj = activator3();

            Assert.IsNotNull(obj);

            Assert.AreEqual(2, obj.TestThis());
        }
    }
}
