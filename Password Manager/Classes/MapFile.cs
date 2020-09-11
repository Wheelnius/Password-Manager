using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

namespace Password_Manager.Classes
{
    class MapFile
    {
        private string Path { get; }
        private Windows.Storage.StorageFile file { get; }
        private LinkedList<Password> PWS;
        private TargetFile tf;
        private Letter[] letters;
        private LinkedList<string> mappings = new LinkedList<string>();
        private string oneLongFuckingString = "";
        

        public MapFile(TargetFile tf)
        {
            this.tf = tf;
            this.letters = tf.getLetters();
        }

        /*
         * Recently changed to String, not void to fix remap items
         */

        public String createMapping(LinkedList<Password> PWS)
        {
            // Uses the linked list used in the listbox to create the mappings.
            // Backwards loop, because items are put into the final string from top to bottom.
            // ALSO THIS LL ITERATION IS DOGSHIT

            this.PWS = PWS;
            for(int i=PWS.Count-1; i>=0; i--)
            {
                MappedPassword finishedMP = createSingleMapping(PWS.ElementAt(i));
                mappings.AddFirst(finishedMP.getID1() + " " + finishedMP.getLineMap());
                oneLongFuckingString += finishedMP.getID1() + " " + finishedMP.getLineMap() + '\n';
                System.Diagnostics.Debug.WriteLine(finishedMP.getID1() + " " + PWS.ElementAt(i).ID2 + " " + finishedMP.getLineMap());
            }

            return oneLongFuckingString;

        }

        public String convertPWStoString(LinkedList<Password> pws)
        {
            String coll = "";
            LinkedListNode<Password> lastPWSNode = pws.Last;
            while(lastPWSNode != null)
            {
                coll += lastPWSNode.Value.ID1 + " " + lastPWSNode.Value.ID2 + '\n';
                lastPWSNode = lastPWSNode.Previous;
            }

            return coll;
        }

        private MappedPassword createSingleMapping(Password pw)
        {
            string[] ID1Arr = pw.ID1.Split(' '); // creates name in the PW file i.e. World_of_Tanks
            string fullID = "";
            for(int i=0; i<ID1Arr.Length; i++)
            {
                if (i + 1 == ID1Arr.Length) fullID += ID1Arr[i];
                else fullID += ID1Arr[i] + "_";
            }

            //System.Diagnostics.Debug.WriteLine(fullID);

            int actualLettersLength = letters.Length;
            for (int i = 0; i < letters.Length; i++)
                if (letters[i] == null)
                {
                    actualLettersLength = i;
                    break;
                }

            MappedPassword MP = new MappedPassword(fullID);

            char[] pwChars = pw.ID2.ToCharArray();
            LetterCoord[] pwCharsCoord = new LetterCoord[pwChars.Length];

            for(int i=0; i<pwChars.Length; i++)
            {
                
                for(int j=0; j<actualLettersLength; j++)
                {
                    try
                    {
                        if (pwChars[i] == letters[j].Let)
                        {
                            
                            LetterCoord tmp = letters[j].useLocCoord();
                            pwCharsCoord[i] = new LetterCoord(tmp.getIntegerNumber(), tmp.getLetterID());
                            break;
                        }
                        
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                        
                    if (j == actualLettersLength - 1)
                    {
                        pwCharsCoord[i] = new LetterCoord(-1, -1);
                        break;
                    }

                }
            }

            MP.createLineMap(pwCharsCoord);

            return MP;
        }

        public string getFileText()
        {
            return oneLongFuckingString;
        }

        public LinkedList<Password> retrievePasswords(string fileText)
        {

            string[] mappings = fileText.Split('\n');
            if (PWS != null)
            {
                PWS.Clear();
            }
            else PWS = new LinkedList<Password>();

           
            for (int i=0; i<mappings.Length; i++)
            {
                // DEBUG
                System.Diagnostics.Debug.WriteLine(mappings[i]);
                Password tmp = retrieveSinglePassword(mappings[i]);
                if (tmp != null)
                {
                    PWS.AddFirst(tmp);
                }

            }
            return PWS;
        }

        private Password retrieveSinglePassword(string mapping)
        {
            string[] tmp = mapping.Split(' ');

            string[] ID1Arr = tmp[0].Split('_');
            string ID1 = "";
            for (int i=0; i<ID1Arr.Length; i++)
            {
                if (i + 1 == ID1Arr.Length) ID1 += ID1Arr[i];
                else ID1 += ID1Arr[i] + " ";
            }

            int actualLettersLength = letters.Length;
            for (int i = 0; i < letters.Length; i++)
                if (letters[i] == null)
                {
                    actualLettersLength = i;
                    break;
                }

            string retPW = "";
            int integerNumber = 0;
            int letterID = 0;

            for (int i=1; i<tmp.Length; i+=2)
            {
                try
                {
                    integerNumber = Int32.Parse(tmp[i]);
                    letterID = Int32.Parse(tmp[i + 1]);

                    if (integerNumber == -1)
                    {
                        retPW += "*";
                        continue;
                    }
                }
                catch (FormatException)
                {
                    continue;
                }
               

                for (int j=0; j<actualLettersLength; j++)
                {
                    try
                    {
                        
                        LinkedList<LetterCoord> tempLetterCoords = letters[j].getLetterCoords();
                        LinkedListNode<LetterCoord> currentNode = tempLetterCoords.First;
                        LetterCoord requiredNode = new LetterCoord(integerNumber, letterID);

                        while ((currentNode != null) && !((currentNode.Value.getIntegerNumber() == integerNumber) && (currentNode.Value.getLetterID() == letterID)))
                            currentNode = currentNode.Next;

                        if (currentNode != null)
                            retPW += letters[j].Let;

                    }
                    catch (NullReferenceException)
                    {
                        retPW += "*";
                        break;
                    }

                }
            }

            return new Password() { ID1 = ID1, ID2 = retPW }; ;
        }
    }
}
