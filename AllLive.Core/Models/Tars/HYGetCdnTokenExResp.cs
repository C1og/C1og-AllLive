using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
    public class HYGetCdnTokenExResp : TarsStruct
    {
        public string sFlvToken { get; set; } = ""; // tag 0
        public int iExpireTime { get; set; } = 0; // tag 1

        public override void ReadFrom(TarsInputStream _is)
        {
            sFlvToken = _is.Read(sFlvToken, 0, isRequire: false);
            iExpireTime = _is.Read(iExpireTime, 1, isRequire: false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(sFlvToken, 0);
            _os.Write(iExpireTime, 1);
        }
    }
}
