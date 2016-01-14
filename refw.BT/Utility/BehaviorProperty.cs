using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public enum BehaviorPropertyType {
        Func, Static, Blackboard, FuncStr
    }

    public class BehaviorPropertyBinding {
        public readonly string PropertyName;
        public readonly Behavior Behavior;

        public BehaviorPropertyBinding(Behavior behavior, string name) {
            this.Behavior = behavior;
            this.PropertyName = name;
        }

        public Type PropertyType {
            get {
                return
                    Behavior.GetBehaviorProperties()
                        .First(p => p.Name == PropertyName)
                        .FieldType.GenericTypeArguments[0];
            }
        }

        public void Set(BasicBehaviorProperty prop) {
            Behavior.GetBehaviorProperties().First(p => p.Name == PropertyName).SetValue(Behavior, prop);
        }

        public void SetStatic<T>(T value) {
            Set(BehaviorProperty<T>.Static(value));
        }

        public void SetBlackboard<T>(string value) {
            var prop = (BasicBehaviorProperty)PropertyType.GetMethod("Blackboard").Invoke(null, new object[] { value });
            Set(prop);
        }

        public void SetFunc<T>(Func<Blackboard, T> value) {
            Set(BehaviorProperty<T>.Func(value));
        }

        public BasicBehaviorProperty Get() {
            return (BasicBehaviorProperty)Behavior.GetBehaviorProperties().First(p => p.Name == PropertyName).GetValue(Behavior);
        }
    }

    public abstract class BasicBehaviorProperty {
        public abstract BehaviorPropertyType GetValueSource();
        public abstract Type GetValueType();
        public abstract object GetLastValue();
    }

    public class BehaviorProperty<T> : BasicBehaviorProperty {
        public T LastValue { get; protected set; }

        private BehaviorPropertyType type;
        private Func<Blackboard, T> funcVal;
        private T staticVal;
        private string variableVal;

        protected BehaviorProperty() { }

        public static BehaviorProperty<T> Func(Func<Blackboard, T> func) {
            var new_prop = new BehaviorProperty<T>();
            new_prop.type = BehaviorPropertyType.Func;
            new_prop.funcVal = func;
            return new_prop;
        }

        public static BehaviorProperty<T> Static(T val) {
            var new_prop = new BehaviorProperty<T>();
            new_prop.type = BehaviorPropertyType.Static;
            new_prop.staticVal = val;
            return new_prop;
        }

        public static BehaviorProperty<T> Blackboard(string variableName) {
            var new_prop = new BehaviorProperty<T>();
            new_prop.type = BehaviorPropertyType.Blackboard;
            new_prop.variableVal = variableName;
            return new_prop;
        }

        public static implicit operator BehaviorProperty<T>(T val) {
            return Static(val);
        }

        public static implicit operator BehaviorProperty<T>(Func<Blackboard, T> func) {
            return Func(func);
        }

        public T GetValue(Blackboard bb) {
            if (type == BehaviorPropertyType.Blackboard) {
                LastValue = bb.Get<T>(variableVal);
                return LastValue;
            } else if (type == BehaviorPropertyType.Func) {
                LastValue = funcVal(bb);
                return LastValue;
            } else if (type == BehaviorPropertyType.Static) {
                LastValue = staticVal;
                return LastValue;
            }

            throw new ArgumentException("BehaviorProperty has an invalid type");
        }

        public override BehaviorPropertyType GetValueSource() {
            return type;
        }

        public override Type GetValueType() {
            return typeof(T);
        }

        public override object GetLastValue() {
            return LastValue;
        }
    }

    public class BehaviorPropertyRef<T> {
        public readonly string Name;
        public BehaviorPropertyRef(string name) {
            Name = name;
        }

        public void Set(Blackboard bb, T val) {
            bb.Set<T>(Name, val);
        }

        public T Get(Blackboard bb) {
            return bb.Get<T>(Name);
        }

        public bool IsSet(Blackboard bb) {
            return bb.Contains(Name) && Get(bb) != null;
        }

        public static implicit operator BehaviorProperty<T>(BehaviorPropertyRef<T> propRef) {
            return BehaviorProperty<T>.Blackboard(propRef.Name);
        }

        public static implicit operator BehaviorPropertyRef<T>(string propRef) {
            return new BehaviorPropertyRef<T>(propRef);
        }
    }
}
