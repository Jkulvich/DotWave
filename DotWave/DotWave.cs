using System;
using System.Collections.Generic;

/*
 * @Jkulvich
 * vk.com/Jkulvich
*/

// 0: chunkID(4) Первые 4 байта должны быть текстом "RIFF" в ANSI кодировке. Т.е. 0x52494646
// 4: chunkSize(4) Должен быть равен длинне всего файла минус 8 байт. Т.е. bytes.Length + (44 - 8) байт.
// 8: format(4) Эти 4 байта должны быть равны тексту "WAVE" 0x57415645
// 12: subchunk1ID(4) Должен быть равен тексту "fmt " 0x666d7420
// 16: subchunk1Size(4) Для формата сжатия PCM (т.е. без сжатия) должен быть равен 16
// 20: audioFormat(2) Для формата PCM (без сжатия) равен 1
// 22: numChannels(2) Количество звуковых дорожек (1-моно, 2-стерео итд).
// 24: sampleRate(4) Частота дискретизации, т.е. сколько байт (UInt32) потребуется на передачу одной секунды звука.
// 28: byteRate(4) Количество байт переданных за секунду воспроизведения ( = sampleRate * numChannels * (bitsPerSample / 8) )
// 32: blockAlign(2) Количество байт для одного сэмпла ( = numChannels * (bitsPerSample / 8) )
// 34: bitsPerSample(2) Количество бит на сэмпл. Сколько бит требуется на одно звуковое значение (8, 16, 24, 32) (1, 2, 3 и 4 байта)
// 36: subchunk2ID(4) Содержит слово "data" 0x64617461
// 40: subchunk2Size(4) Количество байт в области данных
// data(...) Байты данных

namespace LMDDotWave
{
    #region public class DotWaveInfo
    /// <summary>
    /// Содержит полную информацию о звуковом файле
    /// </summary>
    public class DotWaveInfo
    {
        #region DotWaveInfo()
        public DotWaveInfo()
        {
            data = new UInt32[0, 0];
            sampleRate = 44100;
            bitsPerSample = 16;
            RecalcAll();
        }
        #endregion
        #region DotWaveInfo(DotWaveInfo DWI)
        public DotWaveInfo(DotWaveInfo DWI)
        {
            data = DWI.Data;
            bitsPerSample = DWI.BitsPerSample;
            sampleRate = DWI.SampleRate;
            RecalcAll();
        }
        #endregion
        #region DotWaveInfo(UInt32[,] Data, UInt32 SampleRate, UInt16 BitsPerSample)
        public DotWaveInfo(UInt32[,] Data, UInt32 SampleRate, UInt16 BitsPerSample)
        {
            data = Data;
            sampleRate = SampleRate;
            bitsPerSample = BitsPerSample;
            RecalcAll();
        }
        #endregion
        #region private void RecalcAll()
        /// <summary>
        /// Recalcuate all values
        /// </summary>
        private void RecalcAll()
        {
            numChannels = (UInt16)data.GetLength(1);
            blockAlign = (UInt16)(numChannels * (bitsPerSample / 8));
            byteRate = (UInt32)(sampleRate * blockAlign);
            subchunk2Size = (UInt32)(data.GetLength(0) * data.GetLength(1) * (bitsPerSample / 8));
            chunkSize = subchunk2Size + 36;
        }
        #endregion

        UInt32 sampleRate = 44100;
        #region public UInt32 SampleRate
        /// <summary>
        /// Частота дискретизации (обычно 44100)
        /// </summary>
        public UInt32 SampleRate
        {
            get { return sampleRate; }
            set
            {
                sampleRate = value;
                RecalcAll();
            }
        }
        #endregion
        UInt16 bitsPerSample = 16;
        #region public UInt16 BitsPerSample
        /// <summary>
        /// 1(8), 2(16), 3(24) или 4(32) бит на сэмпл (обычно 16)
        /// </summary>
        public UInt16 BitsPerSample
        {
            get { return bitsPerSample; }
            set
            {
                bitsPerSample = value;
                RecalcAll();
            }
        }
        #endregion
        UInt32[,] data = null;
        #region public UInt32[,] Data
        /// <summary>
        /// Данные для аудио. Первое измерение - данные, второе - номер аудио-канала.
        /// </summary>
        public UInt32[,] Data
        {
            get { return data; }
            set
            {
                data = value;
                RecalcAll();
            }
        }
        #endregion

        #region Other data (only read)
        UInt32 chunkID = 0x46464952;
        public UInt32 ChunkID { get { return chunkID; } }
        UInt32 chunkSize = 0;
        public UInt32 ChunkSize { get { return chunkSize; } }
        UInt32 format = 0x45564157;
        public UInt32 Format { get { return format; } }
        UInt32 subchunk1ID = 0x20746d66;
        public UInt32 Subchunk1ID { get { return subchunk1ID; } }
        UInt32 subchunk1Size = 16;
        public UInt32 Subchunk1Size { get { return subchunk1Size; } }
        UInt16 audioFormat = 1;
        public UInt16 AudioFormat { get { return audioFormat; } }
        UInt16 numChannels = 0;
        public UInt16 NumChannels { get { return numChannels; } }
        UInt32 byteRate = 0;        
        public UInt32 ByteRate { get { return byteRate; } }
        UInt16 blockAlign = 0;
        public UInt16 BlockAlign { get { return blockAlign; } }
        UInt32 subchunk2ID = 0x61746164;
        public UInt32 Subchunk2ID { get { return subchunk2ID; } }
        UInt32 subchunk2Size = 0;
        public UInt32 Subchunk2Size { get { return subchunk2Size;  } }
        #endregion

        #region public UInt32 GetDuration()
        /// <summary>
        /// Вернёт длительность аудио в миллисекундах
        /// </summary>
        /// <returns></returns>
        public UInt32 GetDuration() => (UInt32)((float)data.GetLength(0) / sampleRate * 1000f);
        #endregion
    }
    #endregion
    #region public static class DotWave
    /// <summary>
    /// Позволяет читать и записывать данные в .wav файл, а так же проверять, является ли файл волновым
    /// </summary>
    public static class DotWave
    {
        #region public static string IsWave(byte[] bytes)
        /// <summary>
        /// Вернёт null если предоставленный набор байт является аудио формата .wav которое может быть обработано библиотекой.
        /// В противном случае, вернёт текстовую строку с описанием почему именно обработка произойти не может.
        /// </summary>
        /// <param name="bytes">OМассив байт файла .wav</param>
        /// <returns></returns>
        public static string IsWave(byte[] bytes)
        {
            if (bytes.Length < 44) { return "File size less than 44 bytes. It can\'t be."; }
            UInt32 chunkID = BitConverter.ToUInt32(bytes, 0);
            if (chunkID != 0x46464952) { return "File is not have \"RIFF\" chunkID."; }
            UInt32 format = BitConverter.ToUInt32(bytes, 8);
            if (format != 0x45564157) { return "File is not have \"WAVE\" format."; }
            UInt32 subchunk1ID = BitConverter.ToUInt32(bytes, 12);
            if (subchunk1ID != 0x20746d66) { return "File is not have \"fmt \" subchunk1Id."; }
            UInt32 subchunk1Size = BitConverter.ToUInt32(bytes, 16);
            if (subchunk1Size != 16) { return "File have not PCM algorithm in subchunk1Size."; }
            UInt16 audioFormat = BitConverter.ToUInt16(bytes, 20);
            if (subchunk1Size != 16) { return "File have not PCM algorithm in audioFormat."; }
            UInt32 subchunk2ID = BitConverter.ToUInt32(bytes, 36);
            if (subchunk2ID != 0x61746164) { return "File have not \"data\" in subchunk2Id."; }
            return null;
        }
        #endregion
        #region public static DotWaveInfo GetWaves(byte[] bytes)
        /// <summary>
        /// Распарсит массив байт .wav файла и вернёт объект типа DotWaveInfo
        /// </summary>
        /// <param name="bytes">Одномерный массив байт аудио</param>
        /// <returns></returns>
        public static DotWaveInfo GetWaves(byte[] bytes)
        {
            UInt16 numChannels = BitConverter.ToUInt16(bytes, 22);
            UInt32 sampleRate = BitConverter.ToUInt32(bytes, 24);
            UInt16 bitsPerSample = BitConverter.ToUInt16(bytes, 34);

            UInt32[,] WData = new UInt32[(bytes.Length - 44) / (numChannels * (bitsPerSample / 8)), numChannels];

            for (uint block = 0; block < WData.GetLength(0); block++)
            {
                for (uint ch = 0; ch < numChannels; ch++)
                {
                    byte[] part = new byte[4];
                    for (uint i = 0; i < bitsPerSample / 8; i++)
                    {
                        part[i] = bytes[44 + block * (numChannels * (bitsPerSample / 8)) + ch * (bitsPerSample / 8) + i];
                    }
                    if (bitsPerSample <= 8)
                    {
                        if (part[0] < 127) { part[0] += 127; }
                        else { part[0] -= 127; }
                    }
                    UInt32 upart = BitConverter.ToUInt32(part, 0);
                    upart = (UInt32)((double)upart / Math.Pow(256, bitsPerSample / 8) * (double)UInt32.MaxValue);
                    WData[block, ch] = upart;
                }
            }

            return new DotWaveInfo(WData, sampleRate, bitsPerSample);
        }
        #endregion
        #region public static byte[] SetWaves(DotWaveInfo DWI)
        /// <summary>
        /// Соберёт массив байт аудио формата .wav из предоставленного DotWaveInfo
        /// </summary>
        /// <param name="DWI">Объект DotWaveInfo</param>
        /// <returns></returns>
        public static byte[] SetWaves(DotWaveInfo DWI)
        {
            byte[] bytes = new byte[44 + DWI.Data.GetLength(0) * DWI.Data.GetLength(1) * (DWI.BitsPerSample / 8)];

            BitConverter.GetBytes(DWI.ChunkID).CopyTo(bytes, 0);
            BitConverter.GetBytes(DWI.ChunkSize).CopyTo(bytes, 4);
            BitConverter.GetBytes(DWI.Format).CopyTo(bytes, 8);
            BitConverter.GetBytes(DWI.Subchunk1ID).CopyTo(bytes, 12);
            BitConverter.GetBytes(DWI.Subchunk1Size).CopyTo(bytes, 16);
            BitConverter.GetBytes(DWI.AudioFormat).CopyTo(bytes, 20);
            BitConverter.GetBytes(DWI.NumChannels).CopyTo(bytes, 22);
            BitConverter.GetBytes(DWI.SampleRate).CopyTo(bytes, 24);
            BitConverter.GetBytes(DWI.ByteRate).CopyTo(bytes, 28);
            BitConverter.GetBytes(DWI.BlockAlign).CopyTo(bytes, 32);
            BitConverter.GetBytes(DWI.BitsPerSample).CopyTo(bytes, 34);
            BitConverter.GetBytes(DWI.Subchunk2ID).CopyTo(bytes, 36);
            BitConverter.GetBytes(DWI.Subchunk2Size).CopyTo(bytes, 40);

            for (uint block = 1000; block < DWI.Data.GetLength(0); block++)
            {
                for (uint ch = 0; ch < DWI.Data.GetLength(1); ch++)
                {
                    byte[] val = BitConverter.GetBytes((UInt32)((double)DWI.Data[block, ch] / (double)UInt32.MaxValue * Math.Pow(256, DWI.BitsPerSample / 8)));
                    for (int i = 0; i < DWI.BitsPerSample / 8; i++)
                    {
                        if (DWI.BitsPerSample > 8)
                        {
                            bytes[44 + block * DWI.Data.GetLength(1) * (DWI.BitsPerSample / 8) + ch * (DWI.BitsPerSample / 8) + i] = val[i];
                        }
                        else
                        {
                            byte vl = val[0];
                            if (vl < 127) { vl += 127; }
                            else { vl -= 127; }
                            bytes[44 + block * DWI.Data.GetLength(1) * (DWI.BitsPerSample / 8) + ch * (DWI.BitsPerSample / 8) + i] = vl;
                        }
                    }
                }
            }

            return bytes;
        }
        #endregion
    }
    #endregion
    #region public static class DotWaveWorker
    /// <summary>
    /// Позволяет выполнять базовые операции с аудио или звуковыми каналами аудио
    /// </summary>
    public static class DotWaveWorker
    {
        // Сборка и разбиение аудио на каналы
        #region public static UInt32[,] MergeToAudio(Object[] channels) 
        /// <summary>
        /// Склеивает несколько каналов в один аудио массив
        /// </summary>
        /// <param name="channels">Массив массивов из звуковых каналов</param>
        /// <returns></returns>
        public static UInt32[,] MergeToAudio(Object[] channels)
        {
            UInt32[,] audio = new UInt32[((UInt32[])channels[0]).Length, channels.Length];
            for (int ch = 0; ch < channels.Length; ch++)
            {
                for (uint i = 0; i < ((UInt32[])channels[0]).Length; i++)
                {
                    audio[i, ch] = ((UInt32[])channels[ch])[i];
                }
            }
            return audio;
        }
        #endregion
        #region public static Object[] SplitAudio(UInt32[,] audio)
        /// <summary>
        /// Разбирает аудио на звуковые каналы
        /// </summary>
        /// <param name="audio">Двумерный массив аудио</param>
        /// <returns></returns>
        public static Object[] SplitAudio(UInt32[,] audio)
        {
            object[] channels = new object[audio.GetLength(1)];
            for (int ch = 0; ch < channels.Length; ch++)
            {
                UInt32[] channel = new UInt32[audio.GetLength(0)];
                for (uint i = 0; i < channel.Length; i++)
                {
                    channel[i] = audio[i, ch];
                }
                channels[ch] = channel;
            }
            return channels;
        }
        #endregion

        // Работа с каналами в аудио
        #region public static UInt32[] GetChannel(UInt32[,] audio, Int32 num)
        /// <summary>
        /// Возвращает один звуковой-канал из аудио массива
        /// </summary>
        /// <param name="audio">Двумерный массив аудио</param>
        /// <param name="num">Номер звуковой дорожки</param>
        /// <returns></returns>
        public static UInt32[] GetChannel(UInt32[,] audio, Int32 num)
        {
            UInt32[] ch = new UInt32[audio.GetLength(0)];
            for (uint i = 0; i < ch.Length; i++)
            {
                ch[i] = audio[i, num];
            }
            return ch;
        }
        #endregion
        #region public static UInt32[,] AddChannel(UInt32[,] audio, UInt32[] channel)
        /// <summary>
        /// Добавляет канал в конец списка каналов указанного аудио
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="channel">Новый канал</param>
        /// <returns></returns>
        public static UInt32[,] AddChannel(UInt32[,] audio, UInt32[] channel)
        {
            object[] channels = SplitAudio(audio);
            object[] nchannels = new object[channels.Length + 1];
            channels.CopyTo(nchannels, 0);
            nchannels[nchannels.Length - 1] = channel;
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] RemoveChannel(UInt32[,] audio, Int32 num)
        /// <summary>
        /// Удаляет из аудио канал под указанным номером
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="num">Номер канала</param>
        /// <returns></returns>
        public static UInt32[,] RemoveChannel(UInt32[,] audio, Int32 num)
        {
            object[] channels = SplitAudio(audio);
            object[] nchannels = new object[channels.Length - 1];
            int ch = 0;
            for (int i = 0; i < audio.GetLength(1); i++)
            {
                nchannels[ch] = channels[i];
                if (i != num) { ch++; }
            }
            return MergeToAudio(nchannels);
        }
        #endregion
        #region public static UInt32[,] ReplaceChannel(UInt32[,] audio, Int32 num, UInt32[] channel)
        /// <summary>
        /// Заменяет заданный канал в аудио другим каналом
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="num">Номер заменяемого канала</param>
        /// <param name="channel">Массив данных канала</param>
        /// <returns></returns>
        public static UInt32[,] ReplaceChannel(UInt32[,] audio, Int32 num, UInt32[] channel)
        {
            UInt32[,] naudio = new UInt32[audio.GetLength(0), audio.GetLength(1)];
            for (int ch = 0; ch < naudio.GetLength(1); ch++)
            {
                for (uint i = 0; i < channel.Length; i++)
                {
                    if (i != num)
                    {
                        naudio[i, ch] = audio[i, ch];
                        ch++;
                    }
                    else
                    {
                        naudio[i, ch] = channel[i];
                    }
                }
            }
            return naudio;
        }
        #endregion
        #region public static UInt32[,] InsertChannel(UInt32[,] audio, Int32 num, UInt32[] channel)
        /// <summary>
        /// Вставляет аудио-канал в указанную позицию аудио
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="num">Номер позиции</param>
        /// <param name="channel">Массив канала</param>
        /// <returns></returns>
        public static UInt32[,] InsertChannel(UInt32[,] audio, Int32 num, UInt32[] channel)
        {
            object[] channels = SplitAudio(audio);
            object[] nchannels = new object[channels.Length + 1];
            int ch = 0;
            for (int i = 0; i < nchannels.Length; i++)
            {
                if (i != num)
                {
                    nchannels[i] = channels[ch];
                    ch++;
                }
                else
                {
                    nchannels[i] = channel;
                }
            }
            return MergeToAudio(nchannels);
        }
        #endregion

        // Работа с каналами
        #region public static UInt32[] GetSub(UInt32[] channel, UInt32 start, UInt32 length)
        /// <summary>
        /// Получает из канала подканал начиная с заданной позиции и заданной длинны
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <param name="start">Начальная позиция</param>
        /// <param name="length">Длинна</param>
        /// <returns></returns>
        public static UInt32[] GetSub(UInt32[] channel, UInt32 start, UInt32 length)
        {
            UInt32[] subchannel = new UInt32[length];
            for (uint i = 0; i < length; i++)
            {
                subchannel[i] = channel[i + start];
            }
            return subchannel;
        }
        #endregion
        #region public static UInt32[] JoinChannels(Object[] channels)
        /// <summary>
        /// Объединяет несколько каналов в один
        /// </summary>
        /// <param name="channels">Массив аудио-каналов</param>
        /// <returns></returns>
        public static UInt32[] JoinChannels(Object[] channels)
        {
            uint length = 0;
            for (int ch = 0; ch < channels.Length; ch++)
            {
                length += (uint)((UInt32[])channels[ch]).Length;
            }
            UInt32[] nchannel = new UInt32[length];
            uint pos = 0;
            for (int ch = 0; ch < channels.Length; ch++)
            {
                for (uint i = 0; i < ((UInt32[])channels[ch]).Length; i++)
                {
                    nchannel[pos] = ((UInt32[])channels[ch])[i];
                    pos++;
                }
            }
            return nchannel;
        }
        #endregion
        #region public static UInt32[] DeleteSub(UInt32[] channel, UInt32 start, UInt32 length)
        /// <summary>
        /// Удаляет из канала подканал начиная с заданной позиции и заданной длинны
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <param name="start">Начальная позиция</param>
        /// <param name="length">Длинна</param>
        /// <returns></returns>
        public static UInt32[] DeleteSub(UInt32[] channel, UInt32 start, UInt32 length)
        {
            UInt32[] chPre = GetSub(channel, 0, start);
            UInt32[] chPost = GetSub(channel, start + length, (uint)channel.Length - start - length);
            return JoinChannels(new object[] { chPre, chPost });
        }
        #endregion
        #region public static UInt32[] InsertSub(UInt32[] channel, UInt32[] subchannel, UInt32 pos)
        /// <summary>
        /// Вставляет канал внутрь другого канала на заданную позицию
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <param name="subchannel">Добавляемый канал</param>
        /// <param name="pos">Позиция</param>
        /// <returns></returns>
        public static UInt32[] InsertSub(UInt32[] channel, UInt32[] subchannel, UInt32 pos)
        {
            UInt32[] chPre = GetSub(channel, 0, pos);
            UInt32[] chPost = GetSub(channel, pos, (uint)channel.Length - pos);
            return JoinChannels(new object[] { chPre, subchannel, chPost });
        }
        #endregion
        #region public static UInt32[] RewriteSub(UInt32[] channel, UInt32[] subchannel, UInt32 start)
        /// <summary>
        /// Перезаписывает часть байт указанного канала подканалом начиная с заданной позиции
        /// </summary>
        /// <param name="channel">Исходный канал</param>
        /// <param name="subchannel">Подканал</param>
        /// <param name="start">Начальная позиция</param>
        /// <returns></returns>
        public static UInt32[] RewriteSub(UInt32[] channel, UInt32[] subchannel, UInt32 start)
        {
            UInt32[] nchannel = new UInt32[channel.Length];
            for (uint i = 0; i < channel.Length; i++)
            {
                if (i < start || i >= start + subchannel.Length)
                {
                    nchannel[i] = channel[i];
                }
                else
                {
                    nchannel[i] = subchannel[i - start];
                }
            }
            return nchannel;
        }
        #endregion

        // Работа с аудио
        #region public static UInt32[,] GetSub(UInt32[,] audio, UInt32 start, UInt32 length)
        /// <summary>
        /// Возвращает отрезок аудио вырезанный из аудио
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="start">Позиция начала</param>
        /// <param name="length">Длинна</param>
        /// <returns></returns>
        public static UInt32[,] GetSub(UInt32[,] audio, UInt32 start, UInt32 length)
        {
            object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = GetSub((UInt32[])(channels[ch]), start, length);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] JoinAudios(oject[] audios)
        /// <summary>
        /// Возвращает аудио являющееся результатом объединения нескольких аудио
        /// </summary>
        /// <param name="audios">Аудио</param>
        /// <returns></returns>
        public static UInt32[,] JoinAudios(object[] audios)
        {
            uint length = 0;
            for (int au = 0; au < audios.Length; au++)
            {
                length += (UInt32)((UInt32[,])audios[au]).GetLength(0);
            }
            UInt32[,] naudio = new UInt32[length, ((UInt32[,])audios[0]).GetLength(1)];

            for (int ch = 0; ch < ((UInt32[,])audios[0]).GetLength(1); ch++)
            {
                uint pos = 0;
                for (int au = 0; au < audios.Length; au++)
                {
                    for (uint i = 0; i < ((UInt32[,])audios[au]).GetLength(0); i++)
                    {
                        naudio[pos, ch] = ((UInt32[,])audios[au])[i, ch];
                        pos++;
                    }
                }
            }
            return naudio;
        }
        #endregion
        #region public static UInt32[,] DeleteSub(UInt32[,] audio, UInt32 start, UInt32 length)
        /// <summary>
        /// Удаляет часть аудио
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="start">Начальная позиция</param>
        /// <param name="length">Длина</param>
        /// <returns></returns>
        public static UInt32[,] DeleteSub(UInt32[,] audio, UInt32 start, UInt32 length)
        {
            object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = DeleteSub((UInt32[])(channels[ch]), start, length);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] InsertSub(UInt32[,] audio, UInt32[,] subaudio, UInt32 pos)
        /// <summary>
        /// Вставляет аудио в другое аудио
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="subaudio">Аудио которое будет вставлено</param>
        /// <param name="pos">Позиция</param>
        /// <returns></returns>
        public static UInt32[,] InsertSub(UInt32[,] audio, UInt32[,] subaudio, UInt32 pos)
        {
            object[] channels = SplitAudio(audio);
            object[] subchannels = SplitAudio(subaudio);
            for (int ch = 0; ch < audio.GetLength(1); ch++)
            {
                channels[ch] = InsertSub((UInt32[])(channels[ch]), (UInt32[])(subchannels[ch]), pos);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] RewriteSub(UInt32[,] audio, UInt32[,] subaudio, UInt32 pos)
        /// <summary>
        /// Перезаписывает часть байт аудио другим аудио начиная с указанной позиции
        /// </summary>
        /// <param name="audio">Исходный аудио</param>
        /// <param name="subaudio">Аудио которым перезаписываем</param>
        /// <param name="start">Начальная позиция</param>
        /// <returns></returns>
        public static UInt32[,] RewriteSub(UInt32[,] audio, UInt32[,] subaudio, UInt32 start)
        {
            Object[] chs = SplitAudio(audio);
            Object[] schs = SplitAudio(subaudio);
            for (int ch = 0; ch < chs.Length; ch++)
            {
                chs[ch] = RewriteSub((UInt32[])(chs[ch]), (UInt32[])(schs[ch]), start);
            }
            return MergeToAudio(chs);
        }
        #endregion

        // Обработка каналов
        #region public static UInt32[] SetVolume(UInt32[] channel, float vol)
        /// <summary>
        /// Устанавливает коэффициент громкости аудио дорожки
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <param name="vol">Коэффициент</param>
        /// <returns></returns>
        public static UInt32[] SetVolume(UInt32[] channel, float vol)
        {
            UInt32[] nchannel = new UInt32[channel.Length];
            for (uint i = 0; i < channel.Length; i++)
            {
                nchannel[i] = (UInt32)((float)(channel[i] - Int32.MaxValue) * vol - ((float)Int32.MaxValue * vol));
            }
            return nchannel;
        }
        #endregion
        #region public static UInt32[] SetLength(UInt32[] channel, uint length)
        /// <summary>
        /// Устанавливает длинну аудио-дорожки. Дорожка может быть ускоренной или замедленной в зависимости от разницы в длинне
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <param name="length">Длинна возвращаемого массива</param>
        /// <returns></returns>
        public static UInt32[] SetLength(UInt32[] channel, uint length)
        {
            float step = (float)channel.Length / length;
            UInt32[] nchannel = new UInt32[length];
            for (uint i = 0; i < length; i++)
            {
                nchannel[i] = channel[(uint)Math.Truncate(step * i)];
            }
            return nchannel;
        }
        #endregion
        #region public static UInt32[] SetSpeed(UInt32[] channel, float speed)
        /// <summary>
        /// Изменяет скорость аудио-канала
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <param name="speed">Коэффициент скорости</param>
        /// <returns></returns>
        public static UInt32[] SetSpeed(UInt32[] channel, float speed)
        {
            return SetLength(channel, (uint)((float)channel.Length / speed));
        }
        #endregion
        #region public static UInt32[] Merge(UInt32[] ch1, UInt32[] ch2)
        /// <summary>
        /// Объединяет звук двух каналов в один
        /// </summary>
        /// <param name="ch1">Первый канал</param>
        /// <param name="ch2">Второй канал</param>
        /// <returns></returns>
        public static UInt32[] Merge(UInt32[] ch1, UInt32[] ch2)
        {
            UInt32[] nch = new UInt32[ch1.Length];
            for (uint i = 0; i < nch.Length; i++)
            {
                nch[i] = (ch1[i] + ch2[i]) / 2;
            }
            return nch;
        }
        #endregion
        #region public static UInt32[] LinearSoundUp(UInt32[] channel)
        /// <summary>
        /// Линейно повышает громкость звука на канале
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public static UInt32[] LinearSoundUp(UInt32[] channel)
        {
            UInt32[] nchannel = new UInt32[channel.Length];
            float coeff = 1f / channel.Length;
            for (uint i = 0; i < nchannel.Length; i++)
            {
                float vol = coeff * i;
                nchannel[i] = (UInt32)((float)(channel[i] - Int32.MaxValue) * vol - ((float)Int32.MaxValue * vol));
            }
            return nchannel;
        }
        #endregion
        #region public static UInt32[] LinearSoundDown(UInt32[] channel)
        /// <summary>
        /// Линейно понижает громкость звука на канале
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public static UInt32[] LinearSoundDown(UInt32[] channel)
        {
            UInt32[] nchannel = new UInt32[channel.Length];
            float coeff = 1f / channel.Length;
            for (uint i = 0; i < nchannel.Length; i++)
            {
                float vol = coeff * (nchannel.Length - i);
                nchannel[i] = (UInt32)((float)(channel[i] - Int32.MaxValue) * vol - ((float)Int32.MaxValue * vol));
            }
            return nchannel;
        }
        #endregion
        #region public static UInt32[] ParabolicSoundUp(UInt32[] channel)
        /// <summary>
        /// Параболически повышает громкость звука на канале
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public static UInt32[] ParabolicSoundUp(UInt32[] channel)
        {
            UInt32[] nchannel = new UInt32[channel.Length];
            float coeff = 1f / channel.Length;
            for (uint i = 0; i < nchannel.Length; i++)
            {
                float vol = (float)Math.Sin(coeff * i * Math.PI / 2f);
                nchannel[i] = (UInt32)((float)(channel[i] - Int32.MaxValue) * vol - ((float)Int32.MaxValue * vol));
            }
            return nchannel;
        }
        #endregion
        #region public static UInt32[] ParabolicSoundDown(UInt32[] channel)
        /// <summary>
        /// Параболически понижает громкость звука на канале
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public static UInt32[] ParabolicSoundDown(UInt32[] channel)
        {
            UInt32[] nchannel = new UInt32[channel.Length];
            float coeff = 1f / channel.Length;
            for (uint i = 0; i < nchannel.Length; i++)
            {
                float vol = (float)Math.Sin(coeff * (channel.Length - i) * Math.PI / 2f);
                nchannel[i] = (UInt32)((float)(channel[i] - Int32.MaxValue) * vol - ((float)Int32.MaxValue * vol));
            }
            return nchannel;
        }
        #endregion
        #region public static UInt32[] Reverse(UInt32[] channel)
        /// <summary>
        /// Воспроизведение канала в обратную сторону
        /// </summary>
        /// <param name="channel">Канал</param>
        /// <returns></returns>
        public static UInt32[] Reverse(UInt32[] channel)
        {
            UInt32[] nchannel = new UInt32[channel.Length];
            for (uint i = 0; i < channel.Length; i++)
            {
                nchannel[i] = channel[channel.Length - i - 1];
            }
            return nchannel;
        }
        #endregion

        // Обработка аудио
        #region public static UInt32[,] SetVolume(UInt32[,] audio, float vol)
        /// <summary>
        /// Устанавливает коэффициент громкости аудио
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="vol">Коэффициент</param>
        /// <returns></returns>
        public static UInt32[,] SetVolume(UInt32[,] audio, float vol)
        {
            UInt32[,] naudio = new UInt32[audio.GetLength(0), audio.GetLength(1)];
            for (int ch = 0; ch < audio.GetLength(1); ch++)
            {
                for (uint i = 0; i < audio.GetLength(0); i++)
                {
                    naudio[i, ch] = (UInt32)((float)(audio[i, ch] - Int32.MaxValue) * vol - ((float)Int32.MaxValue * vol));
                }
            }
            return naudio;
        }
        #endregion
        #region public static UInt32[,] SetLength(UInt32[,] audio, uint length)
        /// <summary>
        /// Устанавливает длинну аудио. Итоговый аудио может оказаться замедленным или ускоренным, в зависимости от разницы в длинне.
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="length">Итоговая длинна</param>
        /// <returns></returns>
        public static UInt32[,] SetLength(UInt32[,] audio, uint length)
        {
            Object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = SetLength((UInt32[])(channels[ch]), length);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] SetSpeed(UInt32[,] audio, float speed)
        /// <summary>
        /// Устанавливает коэффицеент скорости аудио
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <param name="speed">Коэффициент</param>
        /// <returns></returns>
        public static UInt32[,] SetSpeed(UInt32[,] audio, float speed)
        {
            Object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = SetSpeed((UInt32[])channels[ch], speed);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] Merge(UInt32[,] au1, UInt32[,] au2)
        /// <summary>
        /// Объединяет звук нескольких аудио в один.
        /// </summary>
        /// <param name="au1">Аудио 1</param>
        /// <param name="au2">Аудио 2</param>
        /// <returns></returns>
        public static UInt32[,] Merge(UInt32[,] au1, UInt32[,] au2)
        {
            Object[] chs1 = SplitAudio(au1);
            Object[] chs2 = SplitAudio(au2);
            for (int ch = 0; ch < chs1.Length; ch++)
            {
                chs1[ch] = Merge((UInt32[])chs1[ch], (UInt32[])chs2[ch]);
            }
            return MergeToAudio(chs1);
        }
        #endregion
        #region public static UInt32[,] LinearSoundUp(UInt32[,] audio)
        /// <summary>
        /// Линейно повышает громкость аудио на всех каналах
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <returns></returns>
        public static UInt32[,] LinearSoundUp(UInt32[,] audio)
        {
            Object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = LinearSoundUp((UInt32[])channels[ch]);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] LinearSoundDown(UInt32[,] audio)
        /// <summary>
        /// Линейно понижает громкость аудио на всех каналах
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <returns></returns>
        public static UInt32[,] LinearSoundDown(UInt32[,] audio)
        {
            Object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = LinearSoundDown((UInt32[])channels[ch]);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] ParabolicSoundUp(UInt32[,] audio)
        /// <summary>
        /// Параболически повышает громкость аудио на всех каналах
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <returns></returns>
        public static UInt32[,] ParabolicSoundUp(UInt32[,] audio)
        {
            Object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = ParabolicSoundUp((UInt32[])channels[ch]);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] ParabolicSoundDown(UInt32[,] audio)
        /// <summary>
        /// Параболически понижает громкость аудио на всех каналах
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <returns></returns>
        public static UInt32[,] ParabolicSoundDown(UInt32[,] audio)
        {
            Object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = ParabolicSoundDown((UInt32[])channels[ch]);
            }
            return MergeToAudio(channels);
        }
        #endregion
        #region public static UInt32[,] Reverse(UInt32[,] audio)
        /// <summary>
        /// Воспроизведене аудио в обратную сторону
        /// </summary>
        /// <param name="audio">Аудио</param>
        /// <returns></returns>
        public static UInt32[,] Reverse(UInt32[,] audio)
        {
            Object[] channels = SplitAudio(audio);
            for (int ch = 0; ch < channels.Length; ch++)
            {
                channels[ch] = Reverse((UInt32[])(channels[ch]));
            }
            return MergeToAudio(channels);
        }
        #endregion

        // Генерация каналов
        #region public static UInt32[] GenerateBeep(float freq, uint sampleRate, uint length)
        /// <summary>
        /// Вернёт канал заданной длинны со звуковым писком заданной частоты.
        /// </summary>
        /// <param name="freq">Частота (лучше от 50 до 15000)</param>
        /// <param name="sampleRate">Частота дискретизации аудио для которой предназначается поток (корректировка волны)</param>
        /// <param name="length">Длинна канала</param>
        /// <returns></returns>
        public static UInt32[] GenerateBeep(double freq, uint sampleRate, uint length)
        {
            UInt32[] ch = new UInt32[length];
            for (uint i = 0; i < length; i++)
            {
                double val = Math.Sin(((float)i / length * Math.PI * 2f) * ((float)length / sampleRate * freq));
                if (val > 0) { val -= 1f; }
                else { val += 1f; }
                ch[i] = (uint)((val + 1f) * (UInt32.MaxValue / 2f));
            }
            return ch;
        }
        #endregion
        #region public static UInt32[] GenerateNoise(uint length)
        /// <summary>
        /// Вернёт канал заданной длинны со звуковым шумом.
        /// </summary>
        /// <param name="length">Длинна канала</param>
        /// <returns></returns>
        public static UInt32[] GenerateNoise(uint length)
        {
            UInt32[] ch = new UInt32[length];
            Random ran = new Random();
            for (uint i = 0; i < length; i++)
            {
                ch[i] = (uint)(ran.NextDouble() * UInt32.MaxValue);
            }
            return ch;
        }
        #endregion
        #region public static UInt32[] GenerateInterference(float chance, uint length)
        /// <summary>
        /// Генерирует помехи или хрипы. Указывается шанс таких отклонений.
        /// </summary>
        /// <param name="chance">Шанс 0-1</param>
        /// <param name="length">Длинна</param>
        /// <returns></returns>
        public static UInt32[] GenerateInterference(float chance, uint length)
        {
            UInt32[] ch = new UInt32[length];
            Random ran = new Random();
            for (uint i = 0; i < length; i++)
            {
                if (ran.NextDouble() <= chance)
                {
                    ch[i] = ran.NextDouble() >= 0.5 ? UInt32.MaxValue : 0;
                }
                else
                {
                    ch[i] = UInt32.MaxValue / 2;
                }
            }
            return ch;
        }
        #endregion

        // Готовые шаблоны обработки аудио
        #region public static UInt32[,] Make8Bit(UInt32[,] audio, UInt32 steps)
        public static UInt32[,] Make8Bit(UInt32[,] audio, UInt32 sampleRate, UInt32 Vsteps, UInt32 Hsteps)
        {   
            UInt32[,] naudio = new UInt32[audio.GetLength(0), audio.GetLength(1)];
            uint Vstep = UInt32.MaxValue / Vsteps;
            uint Hstep = sampleRate / Hsteps;
            for (int ch = 0; ch < audio.GetLength(1); ch++)
            {
                for (uint i = 0; i < audio.GetLength(0) / Hstep; i++)
                {
                    ulong value = 0;
                    for (uint ist = 0; ist < Hstep; ist++)
                    {
                        value += audio[i * Hstep + ist, ch];
                    }
                    value = (uint)((double)value / Hstep);
                    for (uint ist = 0; ist < Hstep; ist++)
                    {
                        uint bval = (uint)((Math.Sin(Math.PI * ((ist + i * Hstep) * 0.01f)) * Int32.MaxValue * ((double)value / UInt32.MaxValue)) + Int32.MaxValue); // TODO Отследить среднюю частоту волн
                        if (bval > Int32.MaxValue) { bval -= Int32.MaxValue; }
                        else { bval += Int32.MaxValue; }
                        naudio[i * Hstep + ist, ch] = bval;
                    }
                }
            }
            return naudio;
        }
        #endregion
    }
    #endregion

    #region public static class DotWaveAnalyzer
    /// <summary>
    /// Средство для анализа аудио и совершения более сложных звуковых операций над каналами аудио
    /// </summary>
    public static class DotWaveAnalyzer
    {
        #region public static UInt32[] GetWavesInflection(UInt32[] channel)
        /// <summary>
        /// Получает массив точек перегиба волн
        /// </summary>
        /// <param name="channel">Канал в котором производится поиск точек</param>
        /// <returns></returns>
        public static UInt32[] GetWavesInflection(UInt32[] channel)
        {
            bool goUp = false;
            if (channel[1] - channel[0] > 0) { goUp = true; } else { goUp = false; } // Начальное направление
            uint pointsCount = 0; // Считаем количество волн
            for (uint i = 0; i < channel.Length - 1; i++)
            {
                if ((double)channel[i + 1] - channel[i] > 0 && goUp != true || (double)channel[i + 1] - channel[i] < 0 && goUp != false)
                {
                    goUp = !goUp; // Меняем направление
                    pointsCount++;
                }
            }

            UInt32[] points = new UInt32[pointsCount + 2]; // Берём место под волны
            points[0] = 0;

            uint num = 1;
            for (uint i = 0; i < channel.Length - 1; i++)
            {
                if ((double)channel[i + 1] - channel[i] > 0 && goUp != true || (double)channel[i + 1] - channel[i] < 0 && goUp != false)
                {
                    goUp = !goUp; // Меняем направление
                    points[num] = i;
                    num++;
                }
            }
            points[points.Length - 1] = (uint)channel.Length - 1;

            return points;
        }
        #endregion
        #region public static UInt32[] GetWavesInfo(WaveDesc[] wds)
        public static UInt32[] SetWavesDesc(UInt32[] inflections, UInt32[] ch)
        {
            UInt32[] nchannel = new UInt32[ch.Length];

            uint pos = 0;
            for (uint infl = 0; infl < inflections.Length - 1; infl++)
            {
                float coeff = (float)((float)ch[inflections[infl + 1]] - ch[inflections[infl]]) / (inflections[infl + 1] - inflections[infl]);
                for (uint i = 0; i < inflections[infl + 1] - inflections[infl]; i++)
                {
                    nchannel[pos] = (uint)((float)ch[inflections[infl]] + (float)(coeff * (float)i));
                    //Console.WriteLine( (float)ch[inflections[infl]] + (float)(coeff * (float)i) );
                    pos++;
                }
                //Console.WriteLine();
            }

            return nchannel;
        }
        #endregion
    }
    #endregion
}