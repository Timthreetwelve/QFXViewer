// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace QFXViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        #region Toggle an increased shadow effect when mouse is over Card
        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            Card card = sender as Card;
            ElevationAssist.SetElevation(card, Elevation.Dp6);
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            Card card = sender as Card;
            ElevationAssist.SetElevation(card, Elevation.Dp3);
        }
        #endregion Toggle an increased shadow effect when mouse is over Card
    }
}
