﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 
namespace Amundsen.SSDS.Binding {
    
    
    /// <summary>
    ///Represents a strongly typed in-memory cache of data.
    ///</summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
    [global::System.Serializable()]
    [global::System.ComponentModel.DesignerCategoryAttribute("code")]
    [global::System.ComponentModel.ToolboxItem(true)]
    [global::System.Xml.Serialization.XmlSchemaProviderAttribute("GetTypedDataSetSchema")]
    [global::System.Xml.Serialization.XmlRootAttribute("NewDataSet")]
    [global::System.ComponentModel.Design.HelpKeywordAttribute("vs.data.DataSet")]
    public partial class NewDataSet : global::System.Data.DataSet {
        
        private taskDataTable tabletask;
        
        private global::System.Data.SchemaSerializationMode _schemaSerializationMode = global::System.Data.SchemaSerializationMode.IncludeSchema;
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public NewDataSet() {
            this.BeginInit();
            this.InitClass();
            global::System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new global::System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
            base.Tables.CollectionChanged += schemaChangedHandler;
            base.Relations.CollectionChanged += schemaChangedHandler;
            this.EndInit();
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        protected NewDataSet(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context) : 
                base(info, context, false) {
            if ((this.IsBinarySerialized(info, context) == true)) {
                this.InitVars(false);
                global::System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler1 = new global::System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
                this.Tables.CollectionChanged += schemaChangedHandler1;
                this.Relations.CollectionChanged += schemaChangedHandler1;
                return;
            }
            string strSchema = ((string)(info.GetValue("XmlSchema", typeof(string))));
            if ((this.DetermineSchemaSerializationMode(info, context) == global::System.Data.SchemaSerializationMode.IncludeSchema)) {
                global::System.Data.DataSet ds = new global::System.Data.DataSet();
                ds.ReadXmlSchema(new global::System.Xml.XmlTextReader(new global::System.IO.StringReader(strSchema)));
                if ((ds.Tables["task"] != null)) {
                    base.Tables.Add(new taskDataTable(ds.Tables["task"]));
                }
                this.DataSetName = ds.DataSetName;
                this.Prefix = ds.Prefix;
                this.Namespace = ds.Namespace;
                this.Locale = ds.Locale;
                this.CaseSensitive = ds.CaseSensitive;
                this.EnforceConstraints = ds.EnforceConstraints;
                this.Merge(ds, false, global::System.Data.MissingSchemaAction.Add);
                this.InitVars();
            }
            else {
                this.ReadXmlSchema(new global::System.Xml.XmlTextReader(new global::System.IO.StringReader(strSchema)));
            }
            this.GetSerializationData(info, context);
            global::System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new global::System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
            base.Tables.CollectionChanged += schemaChangedHandler;
            this.Relations.CollectionChanged += schemaChangedHandler;
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.Browsable(false)]
        [global::System.ComponentModel.DesignerSerializationVisibility(global::System.ComponentModel.DesignerSerializationVisibility.Content)]
        public taskDataTable task {
            get {
                return this.tabletask;
            }
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.BrowsableAttribute(true)]
        [global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public override global::System.Data.SchemaSerializationMode SchemaSerializationMode {
            get {
                return this._schemaSerializationMode;
            }
            set {
                this._schemaSerializationMode = value;
            }
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public new global::System.Data.DataTableCollection Tables {
            get {
                return base.Tables;
            }
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.ComponentModel.DesignerSerializationVisibilityAttribute(global::System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public new global::System.Data.DataRelationCollection Relations {
            get {
                return base.Relations;
            }
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        protected override void InitializeDerivedDataSet() {
            this.BeginInit();
            this.InitClass();
            this.EndInit();
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public override global::System.Data.DataSet Clone() {
            NewDataSet cln = ((NewDataSet)(base.Clone()));
            cln.InitVars();
            cln.SchemaSerializationMode = this.SchemaSerializationMode;
            return cln;
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        protected override bool ShouldSerializeTables() {
            return false;
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        protected override bool ShouldSerializeRelations() {
            return false;
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        protected override void ReadXmlSerializable(global::System.Xml.XmlReader reader) {
            if ((this.DetermineSchemaSerializationMode(reader) == global::System.Data.SchemaSerializationMode.IncludeSchema)) {
                this.Reset();
                global::System.Data.DataSet ds = new global::System.Data.DataSet();
                ds.ReadXml(reader);
                if ((ds.Tables["task"] != null)) {
                    base.Tables.Add(new taskDataTable(ds.Tables["task"]));
                }
                this.DataSetName = ds.DataSetName;
                this.Prefix = ds.Prefix;
                this.Namespace = ds.Namespace;
                this.Locale = ds.Locale;
                this.CaseSensitive = ds.CaseSensitive;
                this.EnforceConstraints = ds.EnforceConstraints;
                this.Merge(ds, false, global::System.Data.MissingSchemaAction.Add);
                this.InitVars();
            }
            else {
                this.ReadXml(reader);
                this.InitVars();
            }
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        protected override global::System.Xml.Schema.XmlSchema GetSchemaSerializable() {
            global::System.IO.MemoryStream stream = new global::System.IO.MemoryStream();
            this.WriteXmlSchema(new global::System.Xml.XmlTextWriter(stream, null));
            stream.Position = 0;
            return global::System.Xml.Schema.XmlSchema.Read(new global::System.Xml.XmlTextReader(stream), null);
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        internal void InitVars() {
            this.InitVars(true);
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        internal void InitVars(bool initTable) {
            this.tabletask = ((taskDataTable)(base.Tables["task"]));
            if ((initTable == true)) {
                if ((this.tabletask != null)) {
                    this.tabletask.InitVars();
                }
            }
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private void InitClass() {
            this.DataSetName = "NewDataSet";
            this.Prefix = "";
            this.Locale = new global::System.Globalization.CultureInfo("");
            this.EnforceConstraints = true;
            this.SchemaSerializationMode = global::System.Data.SchemaSerializationMode.IncludeSchema;
            this.tabletask = new taskDataTable();
            base.Tables.Add(this.tabletask);
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private bool ShouldSerializetask() {
            return false;
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private void SchemaChanged(object sender, global::System.ComponentModel.CollectionChangeEventArgs e) {
            if ((e.Action == global::System.ComponentModel.CollectionChangeAction.Remove)) {
                this.InitVars();
            }
        }
        
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static global::System.Xml.Schema.XmlSchemaComplexType GetTypedDataSetSchema(global::System.Xml.Schema.XmlSchemaSet xs) {
            NewDataSet ds = new NewDataSet();
            global::System.Xml.Schema.XmlSchemaComplexType type = new global::System.Xml.Schema.XmlSchemaComplexType();
            global::System.Xml.Schema.XmlSchemaSequence sequence = new global::System.Xml.Schema.XmlSchemaSequence();
            global::System.Xml.Schema.XmlSchemaAny any = new global::System.Xml.Schema.XmlSchemaAny();
            any.Namespace = ds.Namespace;
            sequence.Items.Add(any);
            type.Particle = sequence;
            global::System.Xml.Schema.XmlSchema dsSchema = ds.GetSchemaSerializable();
            if (xs.Contains(dsSchema.TargetNamespace)) {
                global::System.IO.MemoryStream s1 = new global::System.IO.MemoryStream();
                global::System.IO.MemoryStream s2 = new global::System.IO.MemoryStream();
                try {
                    global::System.Xml.Schema.XmlSchema schema = null;
                    dsSchema.Write(s1);
                    for (global::System.Collections.IEnumerator schemas = xs.Schemas(dsSchema.TargetNamespace).GetEnumerator(); schemas.MoveNext(); ) {
                        schema = ((global::System.Xml.Schema.XmlSchema)(schemas.Current));
                        s2.SetLength(0);
                        schema.Write(s2);
                        if ((s1.Length == s2.Length)) {
                            s1.Position = 0;
                            s2.Position = 0;
                            for (; ((s1.Position != s1.Length) 
                                        && (s1.ReadByte() == s2.ReadByte())); ) {
                                ;
                            }
                            if ((s1.Position == s1.Length)) {
                                return type;
                            }
                        }
                    }
                }
                finally {
                    if ((s1 != null)) {
                        s1.Close();
                    }
                    if ((s2 != null)) {
                        s2.Close();
                    }
                }
            }
            xs.Add(dsSchema);
            return type;
        }
        
        public delegate void taskRowChangeEventHandler(object sender, taskRowChangeEvent e);
        
        /// <summary>
        ///Represents the strongly named DataTable class.
        ///</summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
        [global::System.Serializable()]
        [global::System.Xml.Serialization.XmlSchemaProviderAttribute("GetTypedTableSchema")]
        public partial class taskDataTable : global::System.Data.DataTable, global::System.Collections.IEnumerable {
            
            private global::System.Data.DataColumn columnId;
            
            private global::System.Data.DataColumn columnVersion;
            
            private global::System.Data.DataColumn columnname;
            
            private global::System.Data.DataColumn column_is_completed;
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public taskDataTable() {
                this.TableName = "task";
                this.BeginInit();
                this.InitClass();
                this.EndInit();
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            internal taskDataTable(global::System.Data.DataTable table) {
                this.TableName = table.TableName;
                if ((table.CaseSensitive != table.DataSet.CaseSensitive)) {
                    this.CaseSensitive = table.CaseSensitive;
                }
                if ((table.Locale.ToString() != table.DataSet.Locale.ToString())) {
                    this.Locale = table.Locale;
                }
                if ((table.Namespace != table.DataSet.Namespace)) {
                    this.Namespace = table.Namespace;
                }
                this.Prefix = table.Prefix;
                this.MinimumCapacity = table.MinimumCapacity;
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected taskDataTable(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context) : 
                    base(info, context) {
                this.InitVars();
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public global::System.Data.DataColumn IdColumn {
                get {
                    return this.columnId;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public global::System.Data.DataColumn VersionColumn {
                get {
                    return this.columnVersion;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public global::System.Data.DataColumn nameColumn {
                get {
                    return this.columnname;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public global::System.Data.DataColumn _is_completedColumn {
                get {
                    return this.column_is_completed;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            [global::System.ComponentModel.Browsable(false)]
            public int Count {
                get {
                    return this.Rows.Count;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public taskRow this[int index] {
                get {
                    return ((taskRow)(this.Rows[index]));
                }
            }
            
            public event taskRowChangeEventHandler taskRowChanging;
            
            public event taskRowChangeEventHandler taskRowChanged;
            
            public event taskRowChangeEventHandler taskRowDeleting;
            
            public event taskRowChangeEventHandler taskRowDeleted;
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public void AddtaskRow(taskRow row) {
                this.Rows.Add(row);
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public taskRow AddtaskRow(string Id, long Version, string name, bool _is_completed) {
                taskRow rowtaskRow = ((taskRow)(this.NewRow()));
                object[] columnValuesArray = new object[] {
                        Id,
                        Version,
                        name,
                        _is_completed};
                rowtaskRow.ItemArray = columnValuesArray;
                this.Rows.Add(rowtaskRow);
                return rowtaskRow;
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public virtual global::System.Collections.IEnumerator GetEnumerator() {
                return this.Rows.GetEnumerator();
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public override global::System.Data.DataTable Clone() {
                taskDataTable cln = ((taskDataTable)(base.Clone()));
                cln.InitVars();
                return cln;
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected override global::System.Data.DataTable CreateInstance() {
                return new taskDataTable();
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            internal void InitVars() {
                this.columnId = base.Columns["Id"];
                this.columnVersion = base.Columns["Version"];
                this.columnname = base.Columns["name"];
                this.column_is_completed = base.Columns["is-completed"];
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private void InitClass() {
                this.columnId = new global::System.Data.DataColumn("Id", typeof(string), null, global::System.Data.MappingType.Element);
                base.Columns.Add(this.columnId);
                this.columnVersion = new global::System.Data.DataColumn("Version", typeof(long), null, global::System.Data.MappingType.Element);
                base.Columns.Add(this.columnVersion);
                this.columnname = new global::System.Data.DataColumn("name", typeof(string), null, global::System.Data.MappingType.Element);
                base.Columns.Add(this.columnname);
                this.column_is_completed = new global::System.Data.DataColumn("is-completed", typeof(bool), null, global::System.Data.MappingType.Element);
                base.Columns.Add(this.column_is_completed);
                this.columnId.AllowDBNull = false;
                this.columnId.Namespace = "http://schemas.microsoft.com/sitka/2008/03/";
                this.columnVersion.Namespace = "http://schemas.microsoft.com/sitka/2008/03/";
                this.columnname.AllowDBNull = false;
                this.column_is_completed.AllowDBNull = false;
                this.column_is_completed.DefaultValue = ((bool)(false));
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public taskRow NewtaskRow() {
                return ((taskRow)(this.NewRow()));
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected override global::System.Data.DataRow NewRowFromBuilder(global::System.Data.DataRowBuilder builder) {
                return new taskRow(builder);
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected override global::System.Type GetRowType() {
                return typeof(taskRow);
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected override void OnRowChanged(global::System.Data.DataRowChangeEventArgs e) {
                base.OnRowChanged(e);
                if ((this.taskRowChanged != null)) {
                    this.taskRowChanged(this, new taskRowChangeEvent(((taskRow)(e.Row)), e.Action));
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected override void OnRowChanging(global::System.Data.DataRowChangeEventArgs e) {
                base.OnRowChanging(e);
                if ((this.taskRowChanging != null)) {
                    this.taskRowChanging(this, new taskRowChangeEvent(((taskRow)(e.Row)), e.Action));
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected override void OnRowDeleted(global::System.Data.DataRowChangeEventArgs e) {
                base.OnRowDeleted(e);
                if ((this.taskRowDeleted != null)) {
                    this.taskRowDeleted(this, new taskRowChangeEvent(((taskRow)(e.Row)), e.Action));
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            protected override void OnRowDeleting(global::System.Data.DataRowChangeEventArgs e) {
                base.OnRowDeleting(e);
                if ((this.taskRowDeleting != null)) {
                    this.taskRowDeleting(this, new taskRowChangeEvent(((taskRow)(e.Row)), e.Action));
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public void RemovetaskRow(taskRow row) {
                this.Rows.Remove(row);
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public static global::System.Xml.Schema.XmlSchemaComplexType GetTypedTableSchema(global::System.Xml.Schema.XmlSchemaSet xs) {
                global::System.Xml.Schema.XmlSchemaComplexType type = new global::System.Xml.Schema.XmlSchemaComplexType();
                global::System.Xml.Schema.XmlSchemaSequence sequence = new global::System.Xml.Schema.XmlSchemaSequence();
                NewDataSet ds = new NewDataSet();
                global::System.Xml.Schema.XmlSchemaAny any1 = new global::System.Xml.Schema.XmlSchemaAny();
                any1.Namespace = "http://www.w3.org/2001/XMLSchema";
                any1.MinOccurs = new decimal(0);
                any1.MaxOccurs = decimal.MaxValue;
                any1.ProcessContents = global::System.Xml.Schema.XmlSchemaContentProcessing.Lax;
                sequence.Items.Add(any1);
                global::System.Xml.Schema.XmlSchemaAny any2 = new global::System.Xml.Schema.XmlSchemaAny();
                any2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
                any2.MinOccurs = new decimal(1);
                any2.ProcessContents = global::System.Xml.Schema.XmlSchemaContentProcessing.Lax;
                sequence.Items.Add(any2);
                global::System.Xml.Schema.XmlSchemaAttribute attribute1 = new global::System.Xml.Schema.XmlSchemaAttribute();
                attribute1.Name = "namespace";
                attribute1.FixedValue = ds.Namespace;
                type.Attributes.Add(attribute1);
                global::System.Xml.Schema.XmlSchemaAttribute attribute2 = new global::System.Xml.Schema.XmlSchemaAttribute();
                attribute2.Name = "tableTypeName";
                attribute2.FixedValue = "taskDataTable";
                type.Attributes.Add(attribute2);
                type.Particle = sequence;
                global::System.Xml.Schema.XmlSchema dsSchema = ds.GetSchemaSerializable();
                if (xs.Contains(dsSchema.TargetNamespace)) {
                    global::System.IO.MemoryStream s1 = new global::System.IO.MemoryStream();
                    global::System.IO.MemoryStream s2 = new global::System.IO.MemoryStream();
                    try {
                        global::System.Xml.Schema.XmlSchema schema = null;
                        dsSchema.Write(s1);
                        for (global::System.Collections.IEnumerator schemas = xs.Schemas(dsSchema.TargetNamespace).GetEnumerator(); schemas.MoveNext(); ) {
                            schema = ((global::System.Xml.Schema.XmlSchema)(schemas.Current));
                            s2.SetLength(0);
                            schema.Write(s2);
                            if ((s1.Length == s2.Length)) {
                                s1.Position = 0;
                                s2.Position = 0;
                                for (; ((s1.Position != s1.Length) 
                                            && (s1.ReadByte() == s2.ReadByte())); ) {
                                    ;
                                }
                                if ((s1.Position == s1.Length)) {
                                    return type;
                                }
                            }
                        }
                    }
                    finally {
                        if ((s1 != null)) {
                            s1.Close();
                        }
                        if ((s2 != null)) {
                            s2.Close();
                        }
                    }
                }
                xs.Add(dsSchema);
                return type;
            }
        }
        
        /// <summary>
        ///Represents strongly named DataRow class.
        ///</summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
        public partial class taskRow : global::System.Data.DataRow {
            
            private taskDataTable tabletask;
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            internal taskRow(global::System.Data.DataRowBuilder rb) : 
                    base(rb) {
                this.tabletask = ((taskDataTable)(this.Table));
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public string Id {
                get {
                    return ((string)(this[this.tabletask.IdColumn]));
                }
                set {
                    this[this.tabletask.IdColumn] = value;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public long Version {
                get {
                    try {
                        return ((long)(this[this.tabletask.VersionColumn]));
                    }
                    catch (global::System.InvalidCastException e) {
                        throw new global::System.Data.StrongTypingException("The value for column \'Version\' in table \'task\' is DBNull.", e);
                    }
                }
                set {
                    this[this.tabletask.VersionColumn] = value;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public string name {
                get {
                    return ((string)(this[this.tabletask.nameColumn]));
                }
                set {
                    this[this.tabletask.nameColumn] = value;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public bool _is_completed {
                get {
                    return ((bool)(this[this.tabletask._is_completedColumn]));
                }
                set {
                    this[this.tabletask._is_completedColumn] = value;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public bool IsVersionNull() {
                return this.IsNull(this.tabletask.VersionColumn);
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public void SetVersionNull() {
                this[this.tabletask.VersionColumn] = global::System.Convert.DBNull;
            }
        }
        
        /// <summary>
        ///Row event argument class
        ///</summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
        public class taskRowChangeEvent : global::System.EventArgs {
            
            private taskRow eventRow;
            
            private global::System.Data.DataRowAction eventAction;
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public taskRowChangeEvent(taskRow row, global::System.Data.DataRowAction action) {
                this.eventRow = row;
                this.eventAction = action;
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public taskRow Row {
                get {
                    return this.eventRow;
                }
            }
            
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            public global::System.Data.DataRowAction Action {
                get {
                    return this.eventAction;
                }
            }
        }
    }
}
