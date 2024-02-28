using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NssIT.Kiosk.Client.Reports.DsMelakaCentralTicket;

namespace NssIT.Kiosk.Client.Reports
{

    [global::System.Serializable()]
    [global::System.ComponentModel.DesignerCategoryAttribute("code")]
    [global::System.ComponentModel.ToolboxItem(true)]
    [global::System.Xml.Serialization.XmlSchemaProviderAttribute("GetTypedDataSetSchema")]
    [global::System.Xml.Serialization.XmlRootAttribute("DsSkyWayTicket")]
    [global::System.ComponentModel.Design.HelpKeywordAttribute("vs.data.DataSet")]
    public partial class DsSkyWayTicket : global::System.Data.DataSet
    {
        private global::System.Data.SchemaSerializationMode _schemaSerializationMode = global::System.Data.SchemaSerializationMode.IncludeSchema;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]

        public DsSkyWayTicket()
        {
            this.BeginInit();
            this.InitClass();
        }


        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        private void InitClass()
        {
            this.DataSetName = "DsSkyWayTicket";
            this.Prefix = "";
            this.Namespace = "http://tempuri.org/DsMelakaCentralTicket.xsd";
            this.EnforceConstraints = true;
            this.SchemaSerializationMode = global::System.Data.SchemaSerializationMode.IncludeSchema;
            //this.tableTicketInfo = new TicketInfoDataTable();
            //base.Tables.Add(this.tableTicketInfo);
        }
    }

}
