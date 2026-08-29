using System;

namespace Entities
{
    public class AdminKey
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Key { get; set; }
    }
}