using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace Password_Manager.Classes
{
    public class TargetFile
    {
        private Letter[] letters { get; set; }
        private string Path { get; }
        private Windows.Storage.StorageFile file { get; }
        private int actualLettersLength = 0;
        private readonly int TFHash;
        private int entries { get; set; }

        public TargetFile(Windows.Storage.StorageFile file, string Path, int entries)
        {
            this.Path = Path;
            this.file = file;
            this.entries = entries;
            letters = new Letter[95];

            // USING MD5 because I need a numeric hash and I don't need security, i'ts only purpose is to always produce the same int from a string

            String stringName = file.Name.ToString();
            MD5 md5 = MD5.Create();
            byte[] arr = md5.ComputeHash(Encoding.UTF8.GetBytes(stringName));
            int hashCode = BitConverter.ToInt32(arr, 0);

            this.TFHash = hashCode;
            

        }

        public async Task findLetters()
        {
            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
            using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
            {

                for(int i=0; i<entries; i++)
                {            
                    try
                    {
                        int integer = dataReader.ReadInt32();
                        //System.Diagnostics.Debug.WriteLine(integer);
                        integer = Math.Abs(integer);
                        int i1 = integer / 10000000;
                        while (i1 > 128) i1 /= 2;
                        assignLetter((char)i1, i, 1);

                        int i2 = integer / 1000 % 1000;
                        while (i2 > 128) i2 /= 2;
                        assignLetter((char)i2, i, 2);

                        int i3 = integer % 1000;
                        while (i3 > 128) i3 /= 2;
                        assignLetter((char)i3, i, 3);

                    }
                    catch (OverflowException)
                    {
                        //System.Diagnostics.Debug.WriteLine(actualLettersLength + ", Overflow");
                        continue;
                    }
                    catch (Exception)
                    {
                        //System.Diagnostics.Debug.WriteLine(actualLettersLength + ", Exception");
                        //if (actualLettersLength < 129) await new MessageDialog("Characters scanned: " + actualLettersLength + "\nWARNING. File has less than 129 characters. Some password data may not be saved.", "File loaded").ShowAsync();
                        return;
                    }

                }

                //System.Diagnostics.Debug.WriteLine(actualLettersLength + ", Normal");

            }

        }

        public void assignLetter(char letter, int integerNumber, int letterID)
        {
            //System.Diagnostics.Debug.WriteLine(actualLettersLength);
            for (int j = 0; j < letters.Length; j++)
            {

                

                if ((letter > 126) || (letter < 32))
                {
                    letter = (char) 187;
                }

                // IF CHAR ALREADY IN ARRAY
                if (actualLettersLength != 0 && j < actualLettersLength - 1)
                {
                    if (letter == letters[j].Let)
                    {
                        
                        letters[j].addToLetterCoords(integerNumber, letterID);
                        letters[j].addToCount();
                        //System.Diagnostics.Debug.WriteLine("Letter " + letter + " updated, count: " + letters[j].Count);
                        return;
                    }
                }


                // IF CHAR ISN'T IN ARRAY
                if (j + 1 == actualLettersLength && letter != '\n')
                {
                    
                    letters[j] = new Letter(letter, TFHash);
                    letters[j].addToCount();
                    letters[j].addToLetterCoords(integerNumber, letterID);
                    actualLettersLength++;
                    //System.Diagnostics.Debug.WriteLine("Letter " + letter + " added. (Main)");
                    return;
                }
            }

            if(letter != '\n' && actualLettersLength == 0)
            {
                letters[0] = new Letter(letter, TFHash);
                letters[0].addToCount();
                letters[0].addToLetterCoords(integerNumber, letterID);
                actualLettersLength++;
                //System.Diagnostics.Debug.WriteLine("Letter " + letter + " added. (Start)");
                return;
            }
           
        }

        public void refreshLetters()
        {
            for(int i=0; i<letters.Length; i++)
            {
                try
                {
                    letters[i].resetLocCount();
                }
                catch (NullReferenceException)
                {
                    break;
                }
            }
        }

        public Letter[] getLetters()
        {
            return letters;
        }

        public int getActualLettersLength()
        {
            return actualLettersLength;
        }

        public string getPath()
        {
            return Path;
        }

        public int Entries
        {
            get
            {
                return entries;
            }
            set
            {
                this.entries = value;
            }
        }

    }


}
