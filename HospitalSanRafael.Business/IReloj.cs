using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSanRafael.Business
{
    public interface IReloj
    {
        DateTime Ahora { get; }
    }

    public class RelojSistema : IReloj
    {
        public DateTime Ahora { get { return DateTime.Now; } }
    }
}
