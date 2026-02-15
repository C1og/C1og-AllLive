using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
    public class HYGetCdnTokenExReq : TarsStruct
    {
        public string sFlvUrl { get; set; } = ""; // tag 0
        public string sStreamName { get; set; } = ""; // tag 1
        public int iLoopTime { get; set; } = 0; // tag 2
        public HuyaUserId tId { get; set; } = new HuyaUserId(); // tag 3
        public int iAppId { get; set; } = 66; // tag 4

        public override void ReadFrom(TarsInputStream _is)
        {
            sFlvUrl = _is.Read(sFlvUrl, 0, isRequire: false);
            sStreamName = _is.Read(sStreamName, 1, isRequire: false);
            iLoopTime = _is.Read(iLoopTime, 2, isRequire: false);
            var tIdValue = _is.Read(tId, 3, isRequire: false);
            if (tIdValue != null)
            {
                tId = (HuyaUserId)tIdValue;
            }
            iAppId = _is.Read(iAppId, 4, isRequire: false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(sFlvUrl, 0);
            _os.Write(sStreamName, 1);
            _os.Write(iLoopTime, 2);
            _os.Write(tId, 3);
            _os.Write(iAppId, 4);
        }
    }
}
