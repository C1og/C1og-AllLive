using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
    public class HuyaUserId : TarsStruct
    {
        public long lUid { get; set; } = 0; // tag 0
        public string sGuid { get; set; } = ""; // tag 1
        public string sToken { get; set; } = ""; // tag 2
        public string sHuYaUA { get; set; } = ""; // tag 3
        public string sCookie { get; set; } = ""; // tag 4
        public int iTokenType { get; set; } = 0; // tag 5
        public string sDeviceInfo { get; set; } = ""; // tag 6
        public string sQIMEI { get; set; } = ""; // tag 7

        public override void ReadFrom(TarsInputStream _is)
        {
            lUid = _is.Read(lUid, 0, isRequire: false);
            sGuid = _is.Read(sGuid, 1, isRequire: false);
            sToken = _is.Read(sToken, 2, isRequire: false);
            sHuYaUA = _is.Read(sHuYaUA, 3, isRequire: false);
            sCookie = _is.Read(sCookie, 4, isRequire: false);
            iTokenType = _is.Read(iTokenType, 5, isRequire: false);
            sDeviceInfo = _is.Read(sDeviceInfo, 6, isRequire: false);
            sQIMEI = _is.Read(sQIMEI, 7, isRequire: false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(lUid, 0);
            _os.Write(sGuid, 1);
            _os.Write(sToken, 2);
            _os.Write(sHuYaUA, 3);
            _os.Write(sCookie, 4);
            _os.Write(iTokenType, 5);
            _os.Write(sDeviceInfo, 6);
            _os.Write(sQIMEI, 7);
        }
    }
}
