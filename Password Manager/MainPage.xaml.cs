using Password_Manager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Password_Manager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public LinkedList<Password> PWS = new LinkedList<Password>();
        private string targetFilePath;
        private TargetFile tf;
        private MapFile mf;
        private string encPWS = "";
        private int tmpEntriesCount = 10000;

        private Letter[] fullArray;


        public MainPage()
        {
            this.InitializeComponent();
            textBlock3.Text = "";
            ApplicationView.PreferredLaunchViewSize = new Size(630, 668);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;


            Main.Visibility = Visibility.Visible;
            Settings.Visibility = Visibility.Collapsed;
        }

        private async void addPassword(object sender, RoutedEventArgs e)
        {
            string nameValue = textBox.Text;
            string passwordValue = textBox1.Text;

            if(nameValue!=null && passwordValue!=null && passwordValue.Length>=4)
            {
                this.PWS.AddFirst(new Password() { ID1 = nameValue, ID2 = passwordValue });
                textBox.Text = "";
                textBox1.Text = "";
            }

            else
            {
                await new MessageDialog("", "Your password must be at least 4 characers long").ShowAsync();
            }

            this.listBox.ItemsSource = null;
            this.listBox.ItemsSource = PWS;

            if (listBox.Items.Count > 0 && targetFilePath != null)
            {
                button2.IsEnabled = true;
            }
            else button2.IsEnabled = false;
        }

        private void removePassword(object sender, RoutedEventArgs e)
        {
            this.PWS.Remove((Password)listBox.SelectedItem);
            this.listBox.ItemsSource = null;
            this.listBox.ItemsSource = PWS;

            if (listBox.Items.Count > 0 && targetFilePath!=null)
            {
                button2.IsEnabled = true;
            }
            else button2.IsEnabled = false;
        }

        private async void selectTargetFile(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {

                // Removes the "New TF will open..." message
                this.textBlock4.Text = "";

                if (this.tf != null)
                {
                    tf.refreshLetters(); //Refreshes the Linked List counter in previous TargetFile
                }

                this.tf = new TargetFile(file, file.Path, tmpEntriesCount);

                textBlock3.Text = "Finding all characters in the target file, please wait...";
                disableAllControls();
                await Task.Run(() => findLet());
                enableAllControls();
                textBlock3.Text = "";

                targetFilePath = file.Path;
                textBlock1.Text = "Target file: " + targetFilePath;
                button3.IsEnabled = true;

                if(mf != null && PWS.Count > 0)
                {
                    button5.IsEnabled = true;
                }
                else
                {
                    button5.IsEnabled = false;
                }


                if (listBox.Items.Count > 0)
                {
                    button2.IsEnabled = true;
                }
                else button2.IsEnabled = false;

            }
            else
            {
                // do nothing
            }


        }

        private async void createMapping(object sender, RoutedEventArgs e)
        {
            tf.refreshLetters();
            mf = new MapFile(tf);


            textBlock3.Text = "Creating the mapping for passwords...";
            disableAllControls();
            
            try
            {
                await Task.Run(() => createMap());
            }
            catch (Exception)
            {
                await new MessageDialog("Increase the number of allocated entries in settings to eliminate a security threat.", "Mapping file does not contain enough characters not to create any duplicates.").ShowAsync();
                enableAllControls();
                textBlock3.Text = "";
                return;
            }

            enableAllControls();
            textBlock3.Text = "";
            
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Text File (.txt)", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "Not a Passwords Text File";


            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                await Windows.Storage.FileIO.WriteTextAsync(file, mf.getFileText());
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    await new MessageDialog("", "File saved sucessfully.").ShowAsync();
                }
                else
                {
                    await new MessageDialog("", "Failed to save file.").ShowAsync();
                }
            }
        }

        /**
         * Needs live updates on used letters
         * 
         * 
         */

        private async void loadMapping(object sender, RoutedEventArgs e)
        {
            if (this.PWS.Count > 0)
            {
                MessageDialog dialog = new MessageDialog("Opening a new mapping file will delete all current items in the list. Continue?");
                dialog.Commands.Add(new UICommand("Yes", null));
                dialog.Commands.Add(new UICommand("No", null));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var cmd = await dialog.ShowAsync();

                if (cmd.Label == "No")
                {
                    return;
                }
            }

            this.tf.refreshLetters();
            mf = new MapFile(this.tf);


            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".txt");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {

                try
                {
                    this.encPWS = await Windows.Storage.FileIO.ReadTextAsync(file);
                }
                catch (Exception)
                {
                    await new MessageDialog("Are you sure you selected the existing mapping file, and not a target file?", "Failed to load file.").ShowAsync();
                }

                textBlock3.Text = "Retrieving passwords, this may take a while...";
                disableAllControls();
                await Task.Run(() => retPWS(mf));
                enableAllControls();
                textBlock3.Text = "";
                

                if (listBox.Items.Count > 0)
                {
                    button2.IsEnabled = true;
                }
                else button2.IsEnabled = false;

                this.listBox.ItemsSource = null;
                this.listBox.ItemsSource = PWS;

                
                textBlock2.Text = "Mapping file: " + file.Path;

            }
            else
            {
                // do nothing
            }

        }

        private async Task retPWS(MapFile mf)
        {
            PWS = mf.retrievePasswords(encPWS);
            PWS.RemoveFirst();
        }

        private async Task remPWS(MapFile mf)
        {
            String currentList = mf.convertPWStoString(PWS);
            PWS = mf.retrievePasswords(currentList);
            PWS.RemoveFirst();
        }

        private async Task findLet()
        {
            await this.tf.findLetters();

            Letter[] letters = tf.getLetters();
            int lettersLength = letters.Length;

            for (int i = 0; i < letters.Length; i++)
                if (letters[i] == null)
                {
                    lettersLength = i;
                    break;
                }

            for (int i = 0; i < lettersLength; i++)
                tf.getLetters()[i].initScrambleArray();
        }

        private async Task createMap()
        {
            this.encPWS = mf.createMapping(PWS);
        }

        private void disableAllControls()
        {
            button.IsEnabled = false;
            button1.IsEnabled = false;
            button2.IsEnabled = false;
            button3.IsEnabled = false;
            button4.IsEnabled = false;
            button5.IsEnabled = false;

            textBox.IsEnabled = false;
            textBox1.IsEnabled = false;
        }

        private void enableAllControls()
        {
            button.IsEnabled = true;
            button1.IsEnabled = true;
            button2.IsEnabled = true;
            button3.IsEnabled = true;
            button4.IsEnabled = true;
            if (mf != null && PWS.Count > 0) button5.IsEnabled = true;
            else button5.IsEnabled = false;

            textBox.IsEnabled = true;
            textBox1.IsEnabled = true;

        }

        /*
         * Purpose of this method (genuinely just writing this for myself)
         * 1. Get items in the current list (I have the items and their encoded version for the last targetfile)
         * 2. 
         * 
         * 
         * 
         */

        private async void remapItems(object sender, RoutedEventArgs e)
        {
            this.tf.refreshLetters();

            mf = new MapFile(this.tf);
            //PWS.Clear();

            textBlock3.Text = "Retrieving passwords, this may take a while...";
            disableAllControls();
            await Task.Run(() => retPWS(mf));
            enableAllControls();
            textBlock3.Text = "";

            this.listBox.ItemsSource = null;
            this.listBox.ItemsSource = PWS;
        }

        private void settings(object sender, RoutedEventArgs e)
        {
            Main.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Visible;

            // Removes the "New TF will open..." message
            this.textBlock4.Text = "";

            Letter[] letters = null;
            int lettersLength = 0;

            if (tf != null)
            {
                

                try
                {
                    
                    // Checks how many actual letters are in the array
                    letters = tf.getLetters();
                    lettersLength = letters.Length;

                    for (int i=0; i<letters.Length; i++)
                        if (letters[i] == null)
                        {
                            lettersLength = i;
                            break;
                        }
                }
                catch (NullReferenceException) {}

                // Sorts the letters

                Letter[] newList = new Letter[188];
                Letter[] actValues = new Letter[lettersLength];

                int[] values = new int[lettersLength];

                for (int i = 0; i < lettersLength; i++)
                {
                    values[i] = letters[i].Let;
                    newList[values[i]] = letters[i];
                }

                int index = 0;

                for (int i = 0; i < 188; i++)
                {
                    if (newList[i] != null)
                    {
                        actValues[index] = newList[i];
                        index++;
                    }
                }
                    
                fullArray = actValues;

                updateLetterList();
            }



        }

        private void main(object sender, RoutedEventArgs e)
        {
            Main.Visibility = Visibility.Visible;
            Settings.Visibility = Visibility.Collapsed;

            

            int i = 0;
            if (Int32.TryParse(this.textBox2.Text, out i))
            {
                if ((i <= 100000) && (i >= 1))
                {
                    tmpEntriesCount = i;
                    if (tf != null)
                    {
                        if (tf.Entries != tmpEntriesCount) this.textBlock4.Text = "New TF will open with " + tmpEntriesCount + " entries.";

                        // UPDATES THE ENTRIES LIVE IN TF. COULD POSSIBLY CAUSE PROBLEMS
                        //tf.Entries = tmpEntriesCount;
                    }
                    else this.textBlock4.Text = "New TF will open with " + tmpEntriesCount + " entries.";
                }
                else this.textBox2.Text = "" + tmpEntriesCount;
            }
            else this.textBox2.Text = "" + tmpEntriesCount;
        }

        private void updateLetterList()
        {
            this.listBox1.ItemsSource = null;
            this.listBox1.ItemsSource = fullArray;
        }

        private void entriesChanged(object sender, RoutedEventArgs e)
        {

            if (tf != null) tmpEntriesCount = tf.Entries;

            int i = 0;
            if (!Int32.TryParse(this.textBox2.Text, out i))
            {
                this.textBox2.Text = "" + tmpEntriesCount;
            }
            
            if ((i > 100000) || (i < 5000))
            {
                this.textBlock7.Foreground = new SolidColorBrush(Colors.Red);
            } else this.textBlock7.Foreground = new SolidColorBrush(Colors.White);

        }

    }
}
