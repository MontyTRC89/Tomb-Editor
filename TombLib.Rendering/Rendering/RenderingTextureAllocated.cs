using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Rendering
{
    public class RenderingTextureAllocated
    {
        public List<RenderingTextureAllocatorUser> Users = new List<RenderingTextureAllocatorUser>();
        public VectorInt3 Position;

        public void RemoveUser(RenderingTextureAllocatorUser User)
        {
            if (!Users.Remove(User))
                throw new KeyNotFoundException("Rendering texture user not found.");
        }
    }
}
