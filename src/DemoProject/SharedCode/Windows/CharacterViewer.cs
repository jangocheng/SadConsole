﻿using Microsoft.Xna.Framework;

using System;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Effects;

namespace StarterProject.Windows
{
    class CharacterViewer : Window
    {

#region Control Declares
        private SadConsole.Controls.ScrollBar _charScrollBar;
        private Button _closeButton;
#endregion

        private Rectangle trackedRegion = new Rectangle(1, 1, 16, 16);
        private SadConsole.Input.MouseConsoleState lastMouseState = null;
        private SadConsole.Effects.Recolor highlightedEffect;
        private SadConsole.Effects.Fade unhighlightEffect;
        private int fontRowOffset = 0;

        public event EventHandler ColorsChanged;
        public int SelectedCharacterIndex = 0;

        public Color Foreground = Color.White;
        public Color Background = Color.Black;

#region Constructors
        public CharacterViewer()
            : base(27, 20)
        {
            //DefaultShowLocation = StartupLocation.CenterScreen;
            //Fill(Color.White, Color.Black, 0, null);
            Title = (char)198 + "Character" + (char)198;
            TitleAlignment = HorizontalAlignment.Left;
            //SetTitle(" Characters ", HorizontalAlignment.Center, Color.Blue, Color.LightGray);
            CloseOnEscKey = true;
            UsePixelPositioning = true;

            // CHARACTER SCROLL
            _charScrollBar = new ScrollBar(Orientation.Vertical, 16);
            _charScrollBar.Position = new Point(17, 1);
            _charScrollBar.Name = "ScrollBar";
            _charScrollBar.Maximum = Font.Rows - 16;
            _charScrollBar.Value = 0;
            _charScrollBar.ValueChanged += new EventHandler(_charScrollBar_ValueChanged);
            _charScrollBar.IsEnabled = Font.Rows > 16;

            // Add all controls
            this.Add(_charScrollBar);

            _closeButton = new Button(6, 1) { Text = "Ok", Position = new Point(19, 1) }; Add(_closeButton); _closeButton.Click += (sender, e) => { DialogResult = true; Hide(); };

            // Effects
            highlightedEffect = new Recolor();
            highlightedEffect.Foreground = Color.Blue;
            highlightedEffect.Background = Color.DarkGray;

            unhighlightEffect = new SadConsole.Effects.Fade()
            {
                FadeBackground = true,
                FadeForeground = true,
                DestinationForeground = new ColorGradient(highlightedEffect.Foreground, Color.White),
                DestinationBackground = new ColorGradient(highlightedEffect.Background, Color.Black),
                FadeDuration = 0.3d,
                RemoveOnFinished = true,
                UseCellBackground = false,
                UseCellForeground = false,
                CloneOnApply = true,
                Permanent = true
            };

            // The frame will have been drawn by the base class, so redraw and our close button will be put on top of it
            //Invalidate();
        }

#endregion


        void _charScrollBar_ValueChanged(object sender, EventArgs e)
        {
            fontRowOffset = ((SadConsole.Controls.ScrollBar)sender).Value;

            int charIndex = fontRowOffset * 16;
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetGlyph(x, y, charIndex);
                    charIndex++;
                }
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();

            // Draw the border between char sheet area and the text area
            SetGlyph(0, Height - 3, 204);
            SetGlyph(Width - 1, Height - 3, 185);
            for (int i = 1; i < Width - 1; i++)
            {
                SetGlyph(i, Height - 3, 205);
            }
            //SetCharacter(this.Width - 1, 0, 256);

            int charIndex = 0;
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetGlyph(x, y, charIndex);
                    charIndex++;
                }
            }
        }

        protected override void OnMouseLeftClicked(SadConsole.Input.MouseConsoleState state)
        {
            if (state.Cell != null && trackedRegion.Contains(state.ConsoleCellPosition.X, state.ConsoleCellPosition.Y))
            {
                SelectedCharacterIndex = state.Cell.Glyph;
            }
            else if (state.ConsoleCellPosition.X == Width - 1 && state.ConsoleCellPosition.Y == 0)
                Hide();

            base.OnMouseLeftClicked(state);
        }

        protected override void OnMouseMove(SadConsole.Input.MouseConsoleState state)
        {
            if (state.Cell != null && trackedRegion.Contains(state.ConsoleCellPosition.X, state.ConsoleCellPosition.Y))
            {
                // Draw the character index and value in the status area
                string[] items = new string[] { "Index: ", state.Cell.Glyph.ToString() + " ", ((char)state.Cell.Glyph).ToString() };

                items[2] = items[2].PadRight(Width - 2 - (items[0].Length + items[1].Length));

                var text = items[0].CreateColored(Color.LightBlue, Theme.WindowTheme.BorderStyle.Background, null) +
                           items[1].CreateColored(Color.LightCoral, Color.Black, null) +
                           items[2].CreateColored(Color.LightCyan, Color.Black, null);

                text.IgnoreBackground = true;
                text.IgnoreEffect = true;

                Print(1, Height - 2, text);

                // Set the special effect on the current known character and clear it on the last known
                if (lastMouseState == null)
                {
                }
                else if (lastMouseState.ConsoleCellPosition != state.ConsoleCellPosition)
                {
                    SetEffect(lastMouseState.ConsoleCellPosition.X, lastMouseState.ConsoleCellPosition.Y,
                    unhighlightEffect
                    );
                }

                SetEffect(state.ConsoleCellPosition.X, state.ConsoleCellPosition.Y, highlightedEffect);
                lastMouseState = state.Clone();
            }
            else
            {
                DrawSelectedItemString();

                // Clear the special effect on the last known character
                if (lastMouseState != null)
                {
                    SetEffect(lastMouseState.ConsoleCellPosition.X, lastMouseState.ConsoleCellPosition.Y, unhighlightEffect);
                    lastMouseState = null;
                }
            }

            base.OnMouseMove(state);
        }

        private void DrawSelectedItemString()
        {
            // Clear the information area and redraw
            Print(1, Height - 2, "".PadRight(Width - 2));

            //string[] items = new string[] { "Current Index:", SelectedCharacterIndex.ToString() + " ", ((char)SelectedCharacterIndex).ToString() };
            string[] items = new string[] { "Selected: ", ((char)SelectedCharacterIndex).ToString(), " (", SelectedCharacterIndex.ToString(), ")" };
            items[4] = items[4].PadRight(Width - 2 - (items[0].Length + items[1].Length + items[2].Length + items[3].Length));

            var text = items[0].CreateColored(Color.LightBlue, Theme.WindowTheme.BorderStyle.Background, null) +
                       items[1].CreateColored(Color.LightCoral, Theme.WindowTheme.BorderStyle.Background, null) +
                       items[2].CreateColored(Color.LightCyan, Theme.WindowTheme.BorderStyle.Background, null) +
                       items[3].CreateColored(Color.LightCoral, Theme.WindowTheme.BorderStyle.Background, null) +
                       items[4].CreateColored(Color.LightCyan, Theme.WindowTheme.BorderStyle.Background, null);

            text.IgnoreBackground = true;
            text.IgnoreEffect = true;

            Print(1, Height - 2, text);
        }

        protected override void OnMouseExit(SadConsole.Input.MouseConsoleState state)
        {
            if (lastMouseState != null)
            {
//                _lastInfo.Cell.Effect = null;
            }

            DrawSelectedItemString();

            base.OnMouseExit(state);
        }

        protected override void OnMouseEnter(SadConsole.Input.MouseConsoleState state)
        {
            if (lastMouseState != null)
            {
//                _lastInfo.Cell.Effect = null;
            }

            base.OnMouseEnter(state);
        }

        public void UpdateCharSheetColors()
        {
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetForeground(x, y, Foreground);
                    SetBackground(x, y, Background);
                }
            }

            ColorsChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void Show(bool modal)
        {
            if (IsVisible)
                return;

            UpdateCharSheetColors();


            string[] items = new string[] { "Current Index:", SelectedCharacterIndex.ToString() + " ", ((char)SelectedCharacterIndex).ToString() };

            items[2] = items[2].PadRight(Width - 2 - (items[0].Length + items[1].Length));

            var text = items[0].CreateColored(Color.LightBlue, Color.Black, null) +
                       items[1].CreateColored(Color.LightCoral, Color.Black, null) +
                       items[2].CreateColored(Color.LightCyan, Color.Black, null);

            text.IgnoreBackground = true;
            text.IgnoreEffect = true;

            Print(1, Height - 2, text);

            Center();

            base.Show(modal);
        }

    }
}
