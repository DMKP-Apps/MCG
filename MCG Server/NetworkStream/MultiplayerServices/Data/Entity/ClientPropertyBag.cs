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
    public class ClientPropertyBag : Entity
    {
        public ClientPropertyBag() : base() { }

        public ClientPropertyBag(object value) : base() 
        {
            if(value != null)
                this.Value = value.ToJsonString();
        }

        [Key,
        DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity),
        Column("ID", TypeName = "int")]
        public int Id { get; set; }

        [Column("AccountId", TypeName = "nvarchar"), MaxLength(50)]
        public string AccountId { get; set; }

        [ForeignKey("AccountId")]
        public ClientAccount Account { get; set; }

        [Column("PropertyKey", TypeName = "nvarchar"), MaxLength(128), 
        Index()]
        public string Key { get; set; }

        [Column("PropertyValue", TypeName = "ntext")]
        public string Value { get; set; }

        public T Get<T>()
        {
            return this.Get<T>(default(T));
        }
        public T Get<T>(T defaultValue)
        {
            if (string.IsNullOrEmpty(this.Value)) return defaultValue;
            try { return this.Value.FromJsonString<T>(); }
            catch { return defaultValue; }       
        }

        public void Set<T>(T value)
        {
            if (value == null) this.Value = null;
            this.Value = value.ToJsonString();
            this.ModifiedOn = DateTime.Now.ToUniversalTime();
        }



    }
}
