﻿using Gwen.Control;
using System;

namespace Gwen.UnitTest
{
    public class CheckBox : GUnit
    {
        public CheckBox(Base parent)
            : base(parent)
        {

            Control.CheckBox check = new Control.CheckBox(this);
            check.SetPosition(10, 10);
            check.Checked += OnChecked;
            check.UnChecked += OnUnchecked;
            check.CheckChanged += OnCheckChanged;

            Control.LabeledCheckBox labeled = new Control.LabeledCheckBox(this)
            {
                Text = "Labeled CheckBox"
            };
            labeled.Checked += OnChecked;
            labeled.UnChecked += OnUnchecked;
            labeled.CheckChanged += OnCheckChanged;
            Align.PlaceDownLeft(labeled, check, 10);

            Control.LabeledCheckBox labeled2 = new Control.LabeledCheckBox(this)
            {
                Text = "I'm autosized"
            };
            labeled2.SizeToChildren();
            Align.PlaceDownLeft(labeled2, labeled, 10);

            Control.CheckBox check2 = new Control.CheckBox(this)
            {
                IsDisabled = true
            };
            Align.PlaceDownLeft(check2, labeled2, 20);
        }

        private void OnChecked(Base control, EventArgs args)
        {
            UnitPrint("CheckBox: Checked");
        }

        private void OnCheckChanged(Base control, EventArgs args)
        {
            UnitPrint("CheckBox: CheckChanged");
        }

        private void OnUnchecked(Base control, EventArgs args)
        {
            UnitPrint("CheckBox: UnChecked");
        }
    }
}
