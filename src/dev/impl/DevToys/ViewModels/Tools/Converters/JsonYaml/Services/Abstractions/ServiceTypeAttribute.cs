using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;

namespace DevToys.ViewModels.Tools.JsonYaml.Services.Abstractions
{
    class ServiceTypeAttribute : Attribute
    {
        public GeneratorLanguages ServiceId { get; set; }
        public ServiceTypeAttribute(GeneratorLanguages _name)
        {
            ServiceId = _name;
        }
    }
}
