namespace Gwen.Control.Property
{
    /// <summary>
    /// Text property.
    /// </summary>
    public class Text : Base
    {
        protected readonly TextBox m_TextBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Text(Control.Base parent) : base(parent)
        {
            m_TextBox = new TextBox(this)
            {
                Dock = Pos.Fill,
                ShouldDrawBackground = false
            };
            m_TextBox.TextChanged += OnValueChanged;
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get => m_TextBox.Text;
            set => base.Value = value;
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false)
        {
            m_TextBox.SetText(value, fireEvents);
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing => m_TextBox.HasFocus;

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered => base.IsHovered | m_TextBox.IsHovered;
    }
}
