using System;
using System.Collections.Generic;
using System.Text;

namespace tickets
{
    public static class AppSettings
    {
        //Servidor Oficial
        public const string BASE_ADDRESS = "https://cap.unitec.edu";


        //Servidor de Prueba
        //public const string BASE_ADDRESS = "http://157.230.130.35";


        public static int ImagesQuality = 50;
        public static int RefreshTicketsTimeout = 60;
        public const string Encoding = "windows-1252";



        ///<sumary>
        ///Configuraciones del Servido(Parser)
        /// </sumary>
        private static string tableContainer = "//html//body//div//table//table[2]//table//";
        private static string tableContainerHEAD = "//html//body//div//table//table[1]//table//";
        private static string tokenpath = "//html//body//table//";
        private static string temp = "//input[@name='token']";
        private static Dictionary<string, object> configServer = new Dictionary<string, object> {
                {"Token",                               $"//input[@name='token'][@value]" },
                {"Subject",                             $"//html//body//div//table//tr[2]//h3"},

                {"ID de seguimiento",                   $"{tableContainerHEAD}tr[1]//td[2]"},
                {"Estado del ticket",                   $"{tableContainerHEAD}tr[2]//td[2]"},
                {"Creado en",                           $"{tableContainerHEAD}tr[3]//td[2]"},
                {"Actualizar",                          $"{tableContainerHEAD}tr[4]//td[2]"},
                {"Última Respuesta",                    $"{tableContainerHEAD}tr[5]//td[2]"},
                {"Categoria",                           $"{tableContainerHEAD}tr[6]//td[2]"},
                {"Respuestas",                          $"{tableContainerHEAD}tr[7]//td[2]"},
                {"Prioridad",                           $"{tableContainerHEAD}tr[8]//td[2]"},

                {"Fecha",                               $"{tableContainer}table[{1}]//table//tr[1]//td[2]"},
                {"Nombre",                              $"{tableContainer}table[{1}]//table//tr[2]//td[2]"},
                {"E-mail",                              $"{tableContainer}table[{1}]//table//tr[3]//td[2]"},

                {"Campus",                              $"{tableContainer}table[{2}]//tr[1]//td[2]"},
                {"Perfil",                              $"{tableContainer}table[{2}]//tr[2]//td[2]"},
                {"Número de Cuenta",                    $"{tableContainer}table[{2}]//tr[3]//td[2]"},
                {"Carrera | Facultad | Departamento",   $"{tableContainer}table[{2}]//tr[4]//td[2]"},
                {"Clasificación",                       $"{tableContainer}table[{2}]//tr[5]//td[2]"},
                {"Celular | Teléfono",                  $"{tableContainer}table[{2}]//tr[6]//td[2]"},
                {"Correo Personal",                     $"{tableContainer}table[{2}]//tr[7]//td[2]"},

                {"Mensaje",                             $"{tableContainer}p[2]" }
            };

        /// <summary>
        /// Configuracion HTML Parser(print.php)
        /// </summary>
        private static string containerPrint = "//html//body//table//";
        private static Dictionary<string, object> configPrint = new Dictionary<string, object> {
                {"Tema",                                $"{containerPrint}tr[{1}]//td[2]" },
                {"ID de seguimiento",                   $"{containerPrint}tr[{2}]//td[2]"},
                {"Estado del ticket",                   $"{containerPrint}tr[{3}]//td[2]"},
                {"Creado en",                           $"{containerPrint}tr[{4}]//td[2]"},
                {"Actualizar",                          $"{containerPrint}tr[{5}]//td[2]"},
                {"Última respuesta",                    $"{containerPrint}tr[{6}]//td[2]"},
                {"Categoria",                           $"{containerPrint}tr[{7}]//td[2]"},
                {"Nombre",                              $"{containerPrint}tr[{8}]//td[2]"},
                {"Campus",                              $"{containerPrint}tr[{9}]//td[2]"},
                {"Perfil",                              $"{containerPrint}tr[{10}]//td[2]"},
                {"Número de Cuenta",                    $"{containerPrint}tr[{11}]//td[2]"},
                {"Carrera | Facultad | Departamento",   $"{containerPrint}tr[{12}]//td[2]"},
                {"Clasificación",                       $"{containerPrint}tr[{13}]//td[2]"},
                {"Celular | Teléfono",                  $"{containerPrint}tr[{14}]//td[2]"},
                {"Correo Personal",                     $"{containerPrint}tr[{15}]//td[2]"},
                {"Mensaje",                             $"//html//body//p"}
            };
        private static Dictionary<string, Dictionary<string, object>> Configuraciones = new Dictionary<string, Dictionary<string, object>> {
            {"print.php",configPrint },
            {"ticket.php",configServer}
        };
      


        public static Dictionary<string,object> getConfigurationParser(string configuration)
        {
            return Configuraciones[configuration];
        }
    }
}
