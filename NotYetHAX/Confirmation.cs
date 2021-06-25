using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
namespace NotYetHAX
{
    public partial class Confirmation : Form
    {
        public Confirmation()
        { InitializeComponent(); }
        private void playaudio()
        { SoundPlayer audio = new SoundPlayer(Properties.Resources.FormClosing); audio.Play(); }
        private void OK_MouseDown(object sender, MouseEventArgs e)
        { FocusLabel.Focus(); playaudio(); FadeOut.Start(); OpacityChecker.Start(); }
        private void Exit_MouseDown(object sender, MouseEventArgs e)
        { Hide(); }
        private void Exit_MouseEnter(object sender, EventArgs e)
        { Exit.BackColor = Color.Red; }
        private void Exit_MouseLeave(object sender, EventArgs e)
        { Exit.BackColor = Color.Black; }
        private void OpacityChecker_Tick(object sender, EventArgs e)
        {
            if (Opacity == 0)
            { Application.Exit(); }
            else
            { }
        }
        private void FadeOut_Tick(object sender, EventArgs e)
        { Opacity -= 0.01; }
        private void CANCEL_MouseDown(object sender, MouseEventArgs e)
        { FocusLabel.Focus(); Hide(); }
        private void DragButton_MouseDown(object sender, MouseEventArgs e)
        { FocusLabel.Focus(); }
    }
}