using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tickets.API
{
    public class Configuraciones
    {

        /// <summary>
        /// Campus 
        /// </summary>
        private Dictionary<string, object> _campusList = new Dictionary<string, object>() {
            { "Unitec Tegucigalpa",1 },
            { "Unitec San Pedro Sula",2 },
            { "Ceutec Tegucigalpa",3 },
            { "Ceutec La Ceiba",4 },
            { "Unitec DUV",5 }
        };

        public List<string> GetCampus()
        {
            return _campusList.Keys.ToList();
        }

        public string GetCampus(string id)
        {
            foreach(var item in _campusList)
            {
                if (item.Value.ToString() == id)
                {
                    return item.Key;
                }
            }
            return null;
        }

        public string getIdCampus(string name)
        {
            if (_campusList[name] != null)
            {
                return _campusList[name].ToString();
            }
            return null;
        }

        private List<string> _carrerasList = new List<string>()
        {
                    "Arquitectura",
                    "Ingenierí­a en Biomédica",
                    "Ingenierí­a en Civil",
                    "Ingeniería en Energí­a",
                    "Ingenierí­a en Industrial y de Sistemas",
                    "Ingenierí­a en Mecatrónica",
                    "Ingenierí­a en Sistemas Computacionales",
                    "Ingeniería en Telecomunicaciones",
                    "Licenciatura en Administración de Hospitalidad y el Turismo",
                    "Licenciatura en Administración Industrial y de Negocios",
                    "Licenciatura en Animación Digital y Diseño Interactivo",
                    "Licenciatura en Comunicación Audiovisual y Publicitaria",
                    "Licenciatura en Derecho",
                    "Licenciatura en Diseño de Modas",
                    "Licenciatura en Diseño Gráfico",
                    "Licenciatura en Economía",
                    "Licenciatura en Finanzas",
                    "Licenciatura en Gastronomí­a",
                    "Licenciatura en Mercadotecnia y Negocios Internacionales",
                    "Licenciatura en Psicología",
                    "Licenciatura en Relaciones Internacionales",
                    "Medicina y Cirugía",
                    "Nutrición",
                    "Odontología",
                    "Terapia Física y Ocupacional",
                    "Maestrí­a en Administración de Proyectos",
                    "Maestrí­a en Contadurí­a Pública",
                    "Maestrí­a en Derecho Empresarial",
                    "Maestrí­a en Desarrollo Local y Turismo",
                    "Maestrí­a en Dirección de la Comunicación Corporativa",
                    "Maestría en Dirección de Recursos Humanos",
                    "Maestrí­a en Dirección Empresarial",
                    "Maestrí­a en Finanzas",
                    "Maestrí­a en Gestión de Energías Renovables",
                    "Maestría en Gestión de Operaciones y Logística",
                    "Maestrí­a en Gestión de Servicios de Salud",
                    "Maestrí­a en Gestión de Tecnologías de Información",
                    "Maestría en Ingeniería de Estructuras",
                    "Maestrí­a en Sistemas de Gestión de Calidad Integrados",
                    "Doctorado en Economí­a y Empresa"
        };

        public List<string> GetCarreras()
        {
            return _carrerasList;
        }

        private Dictionary<string, object> _perfilesList = new Dictionary<string, object>()
        {
            {"Alumno",1 },
            {"Docente", 2 },
            {"Administrativo",3 }
        };

        public List<string> GetPerfil()
        {
            return _perfilesList.Keys.ToList();
        }

        public string GetIdPerfil(string name)
        {
            if (_perfilesList[name] != null)
                return _perfilesList[name].ToString();

            return null;
        }

        public string GetPerfil(string id)
        {
            foreach (var item in _perfilesList)
            {
                if (item.Value.Equals(id))
                {
                    return item.Key;
                }
            }
            return null;

        }
  

    }
}
