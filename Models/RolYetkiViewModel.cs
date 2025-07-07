using System.Collections.Generic;

namespace PersonelTakip.Models
{
    public class RolYetkiViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionCheckboxItem> Permissions { get; set; }
    }

    public class PermissionCheckboxItem
    {
        public int PermissionId { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool SeciliMi { get; set; } // Checkbox işaretli mi?
    }
}
