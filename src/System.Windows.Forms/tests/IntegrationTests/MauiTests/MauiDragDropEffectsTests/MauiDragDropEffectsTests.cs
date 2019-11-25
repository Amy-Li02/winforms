// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.IntegrationTests.Common;
using Maui.Core;
using ReflectTools;
using WFCTestLib.Util;
using WFCTestLib.Log;

namespace System.Windows.Forms.IntegrationTests.MauiTests
{
    public class MauiDragDropEffectsTests : ReflectBase
    {
        private TextBox _textBox1, _textBox2;
        private Point _ptFrom, _ptTo;
        private TParams _tp;
        private DragDropEffects _ddEffect;

        public MauiDragDropEffectsTests(string[] args) : base(args)
        {
            this.BringToForeground();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Thread.CurrentThread.SetCulture("en-US");
            Application.Run(new MauiDragDropEffectsTests(args));
        }

        [Scenario(true)]
        public ScenarioResult DragDrop_When_DragDropEffects_Is_Copy(TParams p)
        {
            _tp = p;
            Setup();
            _ddEffect = DragDropEffects.Copy;
            Mouse.ClickDrag(MouseFlags.LeftButton, _ptFrom.X + 5, _ptFrom.Y + 5, _ptTo.X + 5, _ptTo.Y + 5);
            Application.DoEvents();

            return new ScenarioResult(string.Equals(_textBox1.Text, _textBox2.Text), "DragDrop is not successful when setting DragDropEffects as Copy.");
        }

        [Scenario(true)]
        public ScenarioResult DragDrop_When_DragDropEffects_Is_None(TParams p)
        {
            _tp = p;
            Setup();
            string str = _textBox1.Text;
            _ddEffect = DragDropEffects.None;
            Mouse.ClickDrag(MouseFlags.LeftButton, _ptFrom.X + 5, _ptFrom.Y + 5, _ptTo.X + 5, _ptTo.Y + 5);
            Application.DoEvents();

            return new ScenarioResult(string.Equals(_textBox1.Text, str) && string.IsNullOrEmpty(_textBox2.Text), "DragDrop is not successful when setting DragDropEffects as None.");
        }

        [Scenario(true)]
        public ScenarioResult DragDrop_When_DragDropEffects_Is_Move(TParams p)
        {
            _tp = p;
            Setup();
            string str = _textBox1.Text;
            _ddEffect = DragDropEffects.Move;
            Mouse.ClickDrag(MouseFlags.LeftButton, _ptFrom.X + 5, _ptFrom.Y + 5, _ptTo.X + 5, _ptTo.Y + 5);
            Application.DoEvents();

            return new ScenarioResult(string.IsNullOrEmpty(_textBox1.Text) && string.Equals(_textBox2.Text, str), "DragDrop is not successful when setting DragDropEffects as Move.");
        }

        [Scenario(true)]
        public ScenarioResult DragDrop_BetweenForms_When_DragDropEffects_Is_Move(TParams p)
        {
            _tp = p;
            Setup();
            Form newForm = new Form();
            Controls.Remove(_textBox2);
            newForm.Controls.Add(_textBox2);
            newForm.Show();
            newForm.Location = new Point(this.Location.X + this.Size.Width, this.Location.Y);
            _ptTo = newForm.PointToScreen(_textBox2.Location);

            string str = _textBox1.Text;
            _ddEffect = DragDropEffects.Move;
            Mouse.ClickDrag(MouseFlags.LeftButton, _ptFrom.X + 5, _ptFrom.Y + 5, _ptTo.X + 5, _ptTo.Y + 5);
            Application.DoEvents();

            return new ScenarioResult(string.IsNullOrEmpty(_textBox1.Text) && string.Equals(_textBox2.Text, str), "DragDrop is not successful between forms when setting DragDropEffects as Move.");
        }

        private void Setup()
        {
            Controls.Clear();
            _textBox1 = new TextBox();
            _textBox2 = new TextBox();

            _textBox1.Location = new Point(0, 0);
            _textBox1.Text = _tp.ru.GetString(10);
            _textBox1.MouseDown += new MouseEventHandler(Tb_MouseDown);

            _textBox2.Location = new Point(0, _textBox1.Size.Height);
            _textBox2.AllowDrop = true;
            _textBox2.DragEnter += new DragEventHandler(Tb_DragEnter);
            _textBox2.DragDrop += new DragEventHandler(Tb_DragDrop);

            Controls.Add(_textBox1);
            Controls.Add(_textBox2);

            _ptFrom = PointToScreen(_textBox1.Location);
            _ptTo = PointToScreen(_textBox2.Location);
        }

        private void Tb_MouseDown(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            DragDropEffects ddeffects = textBox.DoDragDrop(textBox.Text, DragDropEffects.All);
            _tp.log.WriteLine("Start DragDrop with text: " + textBox.Text);
            if (ddeffects == DragDropEffects.Move)
            {
                textBox.Text = "";
            }
        }

        private void Tb_DragEnter(object sender, DragEventArgs e)
        {
            IDataObject data = e.Data;
            if (data.GetDataPresent("Text"))
            {
                e.Effect = _ddEffect;
                _tp.log.WriteLine("The 'Text' data present");
            }
        }

        private void Tb_DragDrop(object sender, DragEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            IDataObject data = e.Data;
            if (data.GetDataPresent("Text"))
            {
                textBox.Text = (string)data.GetData("Text");
                _tp.log.WriteLine("Text Set: " + textBox.Text);
            }
            else
            {
                _tp.log.WriteLine("The 'Text' data doesn't present");
            }
        }
    }
}
