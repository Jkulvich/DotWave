using System;

using System.IO;
using System.Media;
using System.Threading;

namespace LMDDotWave
{
    #region public class DotWaveInfoStream : DotWaveInfo
    public class DotWaveInfoStream : DotWaveInfo
    {
        
    }
    #endregion
    #region public class DotWaveStream
    public class DotWaveStream
    {
        // Основной поток чтения
        private Stream stream = null;

        public DotWaveStream(Stream stream)
        {
            this.stream = stream;
        }
    }
    #endregion
}
