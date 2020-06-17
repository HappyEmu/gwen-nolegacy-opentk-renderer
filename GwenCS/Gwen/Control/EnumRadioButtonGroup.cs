using System;

namespace Gwen.Control
{
    public class EnumRadioButtonGroup<T> : RadioButtonGroup where T : struct, IConvertible
    {
        public EnumRadioButtonGroup(Base parent) : base(parent)
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("T must be an enumerated type!");
            }

            Text = typeof(T).Name;
            for (int i = 0; i < Enum.GetValues(typeof(T)).Length; i++)
            {
                string name = Enum.GetNames(typeof(T))[i];
                LabeledRadioButton lrb = AddOption(name);
                lrb.UserData = Enum.GetValues(typeof(T)).GetValue(i);
            }
        }

        public T SelectedValue
        {
            get => (T)Selected.UserData;
            set
            {
                foreach (Base child in Children)
                {
                    if (child is LabeledRadioButton && child.UserData.Equals(value))
                    {
                        (child as LabeledRadioButton).RadioButton.Press();
                    }
                }
            }
        }
    }
}
