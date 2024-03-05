using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayPro.Aplicaciones.tools
{
    internal class DatosRadiologia
    {

        private Dictionary<string, string[]> _estructuras = new Dictionary<string, string[]>
    {
        { "Craneo", new string[] { "CRÁNEO" } },
        { "Cuello", new string[] { "COLUMNA CERVICAL" } },
        { "Escapula",   new string[] { "ESCÁPULA", "CLAVÍCULA", "TORÁX", "COSTILLAS", "ESTERNÓN" } },
        { "Abdomen",    new string[] { "ABDOMEN", "COLUMNA LUMBAR" } },
        { "Pelvis",     new string[] { "PELVIS", "SACRO", "CADERA" } },
        { "Brazos",     new string[] { "HOMBRO", "CODO", "HÚMERO", "ANTEBRAZO", "MUÑECA", "FALANGES MANO" } },
        { "Femur",      new string[] { "FEMUR", "RODILLA", "TIBIA Y PERONÉ", "TOBILLO", "PIE", "FANLAGES PIE" } }

    };

        private Dictionary<string, string[]> _proyecciones = new Dictionary<string, string[]>
    {
        { "Craneo"  , new string[] { "CRÁNEO PA", "LATERAL DE CRÁNEO", "SENOS PARANASALES", "PA AXIAL DEL CRÁNEO", "AP AXIAL DEL CRÁNEO" } },
        { "Cuello"  , new string[] { "AP DE CERVICALES C1 Y C2", "AP DE CERVICALES C3 - C7", "LATERAL DE CERVICALES", "LATERAL FLEX DE CERVICALES", "LATERAL EXT DE CERVICALES" } },
        { "Escapula"    , new string[] { "LATERAL DE ESCÁPULA", "AP DE ESCÁPULA" } },
        { "Abdomen"     , new string[] { "AP DE ABDOMEN" } },
        { "Pelvis"      , new string[] { "AP DE PELVIS" } },
        { "Brazos"      , new string[] { "AP DE HOMBRO", "OBLICUA AP DE HOMBRO", "AXIAL DE HOMBRO (SRYKER)" }},
        { "Femur"       , new string[] { "AP DE FEMUR", "LATERAL DE FEMUR" }}

    };

        private Dictionary<int, string[]> proyeccionesSeleccionadas = new Dictionary<int, string[]>
    {
        //ESCAPULA
        { 10, new string[] { "LATERAL DE ESCÁPULA", "ESCÁPULA", "CLAVÍCULA", "TORÁX", "COSTILLAS", "ESTERNÓN" } },
        //CLAVICULA 
        { 20, new string[]  {"AP DE CLAVÍCULA","AXIAL DE CLAVÍCULA"} },
        //TORAX 
        { 30, new string[] {"PA DE TORÁX","LATERAL DE TORÁX"} },
        //COSTILLA
        { 40, new string[]  {"PA DE TORÁX","LATERAL DE TORÁX"} },
        //ESTERNON
        { 50, new string[]  {"LATERAL DE ESTERNÓN"} },
        //ABDOMEN
        { 60, new string[]  {"AP DE ABDOMEN"} },
        //C LUMBAR
        { 70, new string[]  {"AP DE COLUMNA LUMBAR","LATERAL LUMBARES"} },
        //PELVIS
        { 80, new string[]  {"AP DE PELVIS"}},
        
        { 81, new string[]  {"AP AXIAL DE SACRO","LAT AXIAL DE SACRO"} },
        //CADERA
        { 90, new string[]  {"AP DE CADERA", "LATERAL DE CADERA"} },
        //TODO LA PARTE DE BRAZO
        { 100, new string[]  {"AP DE HOMBRO", "OBLICUA AP HOMBRO","AXIAL DE HOMBRO" } },
        
        { 101, new string[]  {"AP DE CODO","LATERAL DE CODO"} },
       
        { 102, new string[]  {"AP DE HÚMERO","LATERAL DE HÚMERO"} },
       
        { 103, new string[]  {"AP DE ANTEBRAZO","LATERAL DE ANTEBRAZO"} },

        { 104, new string[]  {"PA DE MUÑERA","LATERAL DE MUÑECA","TANG. MUÑECA (TUNEL CAPIANO)","AXIAL DE MUÑECA"} },

        { 105, new string[]  {"PA DE MANO","LATERAL DE MANO","OBLICUA DE MANO"} },

        { 106, new string[]  {"AP DE PULGAR","LATERAL DE PULGAR","PA DE DEDOS","OBLICUAS DEDOS"} },
        //TODO LA PARTE DE BRAZO
        { 110, new string[]  {"AP DE FEMUR", "LATERAL DE FEMUR"} },

        { 111, new string[]  {"AP DE RODILLA","LATERAL DE RODILLA","AP AXIAL DE RODILLA(BEDERE)"} },

        { 112, new string[]  {"AP DE PIERNA", "LATERAL DE PIERNA"} },

        { 113, new string[]  {"AP DE TOBILLO (MORTAJA)","LATERAL DE TOBILLO","AXIAL CALCÁNEO"} },

        { 114, new string[]  {"AP DE PIE","LATERAL DE PIE","AXIAL CALCÁNEO"} },

        { 115, new string[]  {"AP DE DEDOS (PIE)","OBLICUA DE DEDOS (PIE)","AXIAL CALCÁNEO (PIE)"} },

    };

        public Dictionary<string, string[]> Estructuras => _estructuras;

        public Dictionary<string, string[]> Proyecciones => _proyecciones;

        public Dictionary<int, string[]> SelectProyeccion => proyeccionesSeleccionadas;
    }
}
