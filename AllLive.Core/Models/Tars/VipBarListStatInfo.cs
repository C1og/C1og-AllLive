using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
    public class VipBarListStatInfo : TarsStruct
    {
        public long lPid { get; set; } = 0; // tag 0
        public int iTotal { get; set; } = 0; // tag 1
        public int iTotalNum { get; set; } = 0; // tag 2

        public override void ReadFrom(TarsInputStream _is)
        {
            lPid = _is.Read(lPid, 0, isRequire: false);
            iTotal = _is.Read(iTotal, 1, isRequire: false);
            iTotalNum = _is.Read(iTotalNum, 2, isRequire: false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(lPid, 0);
            _os.Write(iTotal, 1);
            _os.Write(iTotalNum, 2);
        }
    }
}
