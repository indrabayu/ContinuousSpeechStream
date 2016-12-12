using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace App2
{
    public class Wave
    {
        const int LONGINT = 4;
        const int SMALLINT = 2;
        const int INTEGER = 4;
        const int ID_STRING_SIZE = 4;
        const int WAV_RIFF_SIZE = LONGINT + ID_STRING_SIZE;
        const int WAV_FMT_SIZE = (4 * SMALLINT) + (INTEGER * 2) + LONGINT + ID_STRING_SIZE;
        const int WAV_DATA_SIZE = ID_STRING_SIZE + LONGINT;
        const int WAV_HDR_SIZE = WAV_RIFF_SIZE + ID_STRING_SIZE + WAV_FMT_SIZE + WAV_DATA_SIZE;
        const short PCM = 1;
        const int SAMPLE_SIZE = 2;
        int cursor, nSamples;
        public byte[] output;

        public Wave(int sampleRate, short nChannels, short[] data, int start, int end)
        {
            nSamples = end - start + 1;
            cursor = 0;
            output = new byte[nSamples * SMALLINT + WAV_HDR_SIZE];
            buildHeader(sampleRate, nChannels);
            writeData(data, start, end);
        }
        // ------------------------------------------------------------
        private void buildHeader(int sampleRate, short nChannels)
        {
            write("RIFF");
            write(output.Length);
            write("WAVE");
            writeFormat(sampleRate, nChannels);
        }
        // ------------------------------------------------------------
        public void writeFormat(int sampleRate, short nChannels)
        {
            write("fmt ");
            write(WAV_FMT_SIZE - WAV_DATA_SIZE);
            write(PCM);
            write(nChannels);
            write(sampleRate);
            write(nChannels * sampleRate * SAMPLE_SIZE);
            write((short)(nChannels * SAMPLE_SIZE));
            write((short)16);
        }
        // ------------------------------------------------------------
        public void writeData(short[] data, int start, int end)
        {
            write("data");
            write(nSamples * SMALLINT);
            for (int i = start; i <= end; write(data[i++])) ;
        }
        // ------------------------------------------------------------
        private void write(byte b)
        {
            output[cursor++] = b;
        }
        // ------------------------------------------------------------
        private void write(String id)
        {
            if (id.Length != ID_STRING_SIZE) System.Console.Error.WriteLine("String " + id + " must have four characters.");
            else
            {
                for (int i = 0; i < ID_STRING_SIZE; ++i) write((byte)id[i]);
            }
        }
        // ------------------------------------------------------------
        private void write(int i)
        {
            write((byte)(i & 0xFF)); i >>= 8;
            write((byte)(i & 0xFF)); i >>= 8;
            write((byte)(i & 0xFF)); i >>= 8;
            write((byte)(i & 0xFF));
        }
        // ------------------------------------------------------------
        private void write(short i)
        {
            write((byte)(i & 0xFF)); i >>= 8;
            write((byte)(i & 0xFF));
        }
        // ------------------------------------------------------------
        public bool writeToFile(String filename)
        {
            bool ok = false;

            try
            {
                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);
                //System.IO.File.Create(filename);


                //var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                //File path = new File(documentsPath, "scenario7.mp3");
                //FileOutputStream outFile = new FileOutputStream(path);
                //outFile.Write(output);
                //outFile.Flush();
                //outFile.Close();

                //System.IO.File.WriteAllBytes(filename, output);

                //using (var fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                //{
                //    fileStream.Write(output, 0, output.Length);
                //    fileStream.Flush();
                //    fileStream.Close();
                //}

                var fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                var binaryWriter = new System.IO.BinaryWriter(fileStream);
                binaryWriter.Write(output);
                binaryWriter.Flush();
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
                binaryWriter.Close();

                ok = true;
            }
            catch (FileNotFoundException e)
            {
                e.PrintStackTrace();
                ok = false;
            }
            catch (IOException e)
            {
                ok = false;
                e.PrintStackTrace();
            }
            return ok; 
        }

        public List<List<string>> GetText()
        {
            return ConsumeWCF.Simple_WCF_Client.GetText(output);
        }

        public string GetTime()
        {
            return ConsumeWCF.Simple_WCF_Client.GetTime();
        }

        public string TranscribeUsingGoogle()
        {
            return ConsumeWCF.Simple_WCF_Client.GoogleSpeechV2Using16KHz16BitMonoWave(output);
        }
    }

}