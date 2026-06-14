using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
    public class VipListStatReq : TarsStruct
    {
        public HuyaUserId tUserId { get; set; } = new HuyaUserId(); // tag 0
        public long lPid { get; set; } = 0; // tag 1

        public override void ReadFrom(TarsInputStream _is)
        {
            var tUserIdValue = _is.Read(tUserId, 0, isRequire: false);
            if (tUserIdValue != null)
            {
                tUserId = (HuyaUserId)tUserIdValue;
            }
            lPid = _is.Read(lPid, 1, isRequire: false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(tUserId, 0);
            _os.Write(lPid, 1);
        }
    }
}
