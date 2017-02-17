using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketServer.Library;

namespace MultiplayerServices.Data.Entity
{
    public class ClientAccount : Entity
    {
        public ClientAccount() : base()
        {
            this.IsOnline = false;
            this.IsLockedOut = false;
        }

        [Key, Column("ID", TypeName="nvarchar"), MaxLength(50)]
        public string Id { get; set; }

        [Required(ErrorMessage="Invalid account setting, a username is required."), 
        MaxLength(128, ErrorMessage="Account username exceeds the limit of 128 characters.")]
        public string Username { get; set; }

        public bool IsOnline { get; set; }

        public DateTime? LastRequestOn { get; set; }

        public bool IsLockedOut { get; set; }

        public DateTime? LockedOutOn { get; set; }

        public virtual ICollection<ClientPropertyBag> Properties { get; set; }


        public IEnumerable<T> GetProperty<T>(string key)
        {
            if (!string.IsNullOrEmpty(key)) return new List<T>();

            return this.Properties.Where(x => x.Key == key).Select(result => result.Get<T>())
                .Where(x => x != null);
        }


    }
}
