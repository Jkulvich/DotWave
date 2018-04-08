using System;

using System.IO;
using System.Media;
using LMDDotWave;

using A = LMDDotWave.DotWaveWorker;

class Program
{
    static void Main()
    {
        /*TextWriter console = Console.Out;
        Stream S = new FileStream("log.txt", FileMode.OpenOrCreate);
        TextWriter TW = new StreamWriter(S);
        Console.SetOut(TW);*/

        /*Byte[] bwav = File.ReadAllBytes("SPIRIT.wav");
        DotWaveInfo DWI = DotWave.GetWaves(bwav);

        UInt32[] ch = DotWaveWorker.GetChannel(DWI.Data, 0);
        UInt32[] wds = DotWaveAnalyzer.GetWavesInflection(ch);
        UInt32[] nch = DotWaveAnalyzer.SetWavesDesc(wds, ch);
        DotWaveInfo DWI2 = new DotWaveInfo(DotWaveWorker.MergeToAudio(new object[] { nch }), 44100, 16);

        Byte[] nbwav = DotWave.SetWaves(DWI2);
        new SoundPlayer(new MemoryStream(nbwav)).Play();
        File.WriteAllBytes("out.wav", nbwav);*/

        //Console.SetOut(console);

        /*Stream stream = new MemoryStream();
        DotWaveStream DWS = new DotWaveStream(stream);

        Console.WriteLine("end");*/
        Console.ReadKey();
    }
}