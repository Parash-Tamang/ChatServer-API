import React, { useState, useMemo } from 'react';
import { 
  Copy, Send, Check, Plus, X, Trash2, 
  Database, Table as TableIcon, ShieldCheck, 
  ShieldAlert, Filter, Info, Search, Sun, Moon
} from 'lucide-react';

const SCHEMA = [
  { schemaName: "dbo", tableName: "BuildVersion", columns: ["SystemInformationID", "Database Version", "VersionDate", "ModifiedDate"] },
  { schemaName: "dbo", tableName: "ErrorLog", columns: ["ErrorLogID", "ErrorTime", "UserName", "ErrorNumber", "ErrorSeverity", "ErrorState", "ErrorProcedure", "ErrorLine", "ErrorMessage"] },
  { schemaName: "SalesLT", tableName: "Address", columns: ["AddressID", "AddressLine1", "AddressLine2", "City", "StateProvince", "CountryRegion", "PostalCode", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "Customer", columns: ["CustomerID", "NameStyle", "Title", "FirstName", "MiddleName", "LastName", "Suffix", "CompanyName", "SalesPerson", "EmailAddress", "Phone", "PasswordHash", "PasswordSalt", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "CustomerAddress", columns: ["CustomerID", "AddressID", "AddressType", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "Product", columns: ["ProductID", "Name", "ProductNumber", "Color", "StandardCost", "ListPrice", "Size", "Weight", "ProductCategoryID", "ProductModelID", "SellStartDate", "SellEndDate", "DiscontinuedDate", "ThumbNailPhoto", "ThumbnailPhotoFileName", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "ProductCategory", columns: ["ProductCategoryID", "ParentProductCategoryID", "Name", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "ProductDescription", columns: ["ProductDescriptionID", "Description", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "ProductModel", columns: ["ProductModelID", "Name", "CatalogDescription", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "ProductModelProductDescription", columns: ["ProductModelID", "ProductDescriptionID", "Culture", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "SalesOrderDetail", columns: ["SalesOrderID", "SalesOrderDetailID", "OrderQty", "ProductID", "UnitPrice", "UnitPriceDiscount", "LineTotal", "rowguid", "ModifiedDate"] },
  { schemaName: "SalesLT", tableName: "SalesOrderHeader", columns: ["SalesOrderID", "RevisionNumber", "OrderDate", "DueDate", "ShipDate", "Status", "OnlineOrderFlag", "SalesOrderNumber", "PurchaseOrderNumber", "AccountNumber", "CustomerID", "ShipToAddressID", "BillToAddressID", "ShipMethod", "CreditCardApprovalCode", "SubTotal", "TaxAmt", "Freight", "TotalDue", "Comment", "rowguid", "ModifiedDate"] }
];

export default function PermissionSetCreator() {
  const [roleName, setRoleName] = useState('');
  const [permissions, setPermissions] = useState({});
  const [copied, setCopied] = useState(false);
  const [selectedSchemaFilter, setSelectedSchemaFilter] = useState('dbo');
  const [schemaSearch, setSchemaSearch] = useState('');
  const [tableSearch, setTableSearch] = useState('');
  const [isDarkMode, setIsDarkMode] = useState(false);

  const getTableKey = (schema, table) => `${schema}.${table}`;

  const getColumns = (key) => {
    const [s, ...rest] = key.split('.');
    const t = rest.join('.');
    const row = SCHEMA.find(x => x.schemaName === s && x.tableName === t);
    return row ? row.columns : [];
  };

  const handleAddTable = (schema, table) => {
    const key = getTableKey(schema, table);
    setPermissions(prev => ({
      ...prev,
      [key]: {
        reason: '',
        access_level: 'unrestricted',
        required_filters: [],
        isSaved: false,
        hasBeenSaved: false
      }
    }));
  };

  const handleRemoveTable = (key) => {
    setPermissions(prev => {
      const next = { ...prev };
      delete next[key];
      return next;
    });
  };

  const handleUpdateAccess = (key, level) => {
    setPermissions(prev => ({
      ...prev,
      [key]: {
        ...prev[key],
        access_level: level,
        required_filters: level === 'filtered' ? prev[key].required_filters : []
      }
    }));
  };

  const handleUpdateReason = (key, reason) => {
    setPermissions(prev => ({
      ...prev,
      [key]: { ...prev[key], reason }
    }));
  };

  const handleSaveTable = (key) => {
    setPermissions(prev => ({
      ...prev,
      [key]: { ...prev[key], isSaved: true, hasBeenSaved: true }
    }));
  };

  const handleEditTable = (key) => {
    setPermissions(prev => ({
      ...prev,
      [key]: { ...prev[key], isSaved: false }
    }));
  };

  const handleAddFilter = (key) => {
    const cols = getColumns(key);
    setPermissions(prev => {
      const currentFilters = prev[key].required_filters || [];
      const usedColumns = currentFilters.map(f => f.column);
      const availableColumn = cols.find(c => !usedColumns.includes(c));

      // Don't add if all columns are already used
      if (!availableColumn) return prev;

      return {
        ...prev,
        [key]: {
          ...prev[key],
          required_filters: [
            ...currentFilters,
            { column: availableColumn, filter_type: 'id', values: '' }
          ]
        }
      };
    });
  };

  const handleRemoveFilter = (key, index) => {
    setPermissions(prev => ({
      ...prev,
      [key]: {
        ...prev[key],
        required_filters: prev[key].required_filters.filter((_, i) => i !== index)
      }
    }));
  };

  const handleUpdateFilter = (key, index, field, val) => {
    setPermissions(prev => {
      const newFilters = [...prev[key].required_filters];
      newFilters[index] = { ...newFilters[index], [field]: val };
      return {
        ...prev,
        [key]: { ...prev[key], required_filters: newFilters }
      };
    });
  };

  // Build the final payload matching the original structure
  const payload = useMemo(() => {
    const out = {
      role: roleName.trim() || 'unnamed_role',
      permissions: {}
    };

    Object.entries(permissions).forEach(([key, perm]) => {
      // Only include tables that have been saved in the final payload
      if (!perm.hasBeenSaved) return;

      // Process filter values to match the original logic (arrays for comma-separated)
      const processedFilters = perm.required_filters.map(f => {
        let finalValues = f.values;
        if (typeof f.values === 'string') {
           const parts = f.values.split(',').map(v => v.trim()).filter(Boolean);
           finalValues = parts.length === 1 ? parts[0] : parts;
           if (parts.length === 0) finalValues = "";
        }
        return { ...f, values: finalValues };
      });

      out.permissions[key] = {
        reason: perm.reason,
        access_level: perm.access_level,
        required_filters: processedFilters
      };
    });
    return out;
  }, [roleName, permissions]);

  const handleCopyPayload = () => {
    navigator.clipboard.writeText(JSON.stringify(payload, null, 2)).catch(() => {});
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleSendPayload = () => {
    alert("Payload Review:\n\n" + JSON.stringify(payload, null, 2));
  };

  const addedCount = Object.keys(permissions).length;
  const tablesToConfigure = Object.entries(permissions).filter(([_, perm]) => !perm.isSaved);
  
  const uniqueSchemas = useMemo(() => Array.from(new Set(SCHEMA.map(t => t.schemaName))), []);
  const filteredSchemas = useMemo(() => uniqueSchemas.filter(s => s.toLowerCase().includes(schemaSearch.toLowerCase())), [uniqueSchemas, schemaSearch]);
  
  const filteredTables = useMemo(() => {
    const baseFiltered = SCHEMA.filter(t => 
      t.schemaName === selectedSchemaFilter && 
      t.tableName.toLowerCase().includes(tableSearch.toLowerCase())
    );

    // Sort modified/saved tables to the beginning of the list
    return baseFiltered.sort((a, b) => {
      const keyA = getTableKey(a.schemaName, a.tableName);
      const keyB = getTableKey(b.schemaName, b.tableName);
      
      const permA = permissions[keyA];
      const permB = permissions[keyB];

      const isModifiedA = permA ? (permA.reason.trim() !== '' || permA.access_level !== 'unrestricted' || permA.required_filters.length > 0 || permA.hasBeenSaved) : false;
      const isModifiedB = permB ? (permB.reason.trim() !== '' || permB.access_level !== 'unrestricted' || permB.required_filters.length > 0 || permB.hasBeenSaved) : false;

      if (isModifiedA && !isModifiedB) return -1;
      if (!isModifiedA && isModifiedB) return 1;
      return 0; // Maintain original alphabetical order if both are in the same state
    });
  }, [selectedSchemaFilter, tableSearch, permissions]);

  return (
    <div className={isDarkMode ? 'dark' : ''}>
      <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-200 p-4 md:p-8 font-sans transition-colors duration-200">
        <div className="max-w-5xl mx-auto space-y-8">
          
          {/* Header */}
          <div className="mb-6 flex items-start justify-between">
            <div>
              <h1 className="text-2xl font-bold flex items-center gap-2">
                <ShieldCheck className="text-indigo-600 dark:text-indigo-400" />
                Permission Set Creator
              </h1>
              <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                Build role-based table permissions and export as a JSON payload.
              </p>
            </div>
            <button
              onClick={() => setIsDarkMode(!isDarkMode)}
              className="p-2.5 rounded-full bg-slate-200 dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-300 dark:hover:bg-slate-700 transition-colors"
              title="Toggle dark mode"
            >
              {isDarkMode ? <Sun size={18} /> : <Moon size={18} />}
            </button>
          </div>

          {/* Step 1: Role Name */}
          <section className="bg-white dark:bg-slate-900 p-6 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800">
            <h2 className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider mb-4 flex items-center gap-2">
              <span className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Step 1</span> 
              Role Name
            </h2>
            <div className="flex items-center gap-3">
              <input 
                type="text" 
                placeholder="e.g. customer, admin, vendor"
                value={roleName}
                onChange={(e) => setRoleName(e.target.value)}
                className="w-full max-w-xs px-4 py-2 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition-all text-sm"
              />
            </div>
          </section>

          {/* Step 2: Select Schema */}
          <section className="bg-white dark:bg-slate-900 p-6 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-4">
              <h2 className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider flex items-center gap-2">
                <span className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Step 2</span> 
                Select Schema
              </h2>
              
              <div className="relative">
                <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400" size={14} />
                <input 
                  type="text"
                  placeholder="Search schema..."
                  value={schemaSearch}
                  onChange={(e) => setSchemaSearch(e.target.value)}
                  className="pl-8 pr-3 py-1.5 text-sm bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-700 rounded-md focus:ring-2 focus:ring-indigo-500 outline-none w-full sm:w-48 transition-all"
                />
              </div>
            </div>
            
            <div className="flex flex-wrap gap-2.5">
              {filteredSchemas.length > 0 ? (
                filteredSchemas.map(s => (
                  <button
                    key={s}
                    onClick={() => setSelectedSchemaFilter(s)}
                    className={`flex items-center gap-2 px-3.5 py-2 rounded-full text-sm font-medium transition-all border ${
                      selectedSchemaFilter === s 
                        ? 'bg-indigo-600 text-white border-indigo-600 shadow-sm'
                        : 'bg-white text-slate-600 border-slate-200 shadow-sm hover:border-indigo-300 hover:bg-indigo-50 dark:bg-slate-900 dark:text-slate-300 dark:border-slate-700 dark:hover:border-indigo-500 dark:hover:bg-indigo-900/20'
                    }`}
                  >
                    {s}
                    {selectedSchemaFilter === s && <Check size={14} className="opacity-100" />}
                  </button>
                ))
              ) : (
                <span className="text-sm text-slate-500 px-3 py-1.5 font-medium">No schemas match</span>
              )}
            </div>
          </section>

          {/* Step 3: Select Tables */}
          <section className="bg-white dark:bg-slate-900 p-6 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-4">
              <div className="flex flex-wrap items-center gap-4">
                <h2 className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider flex items-center gap-2">
                  <span className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Step 3</span> 
                  Select Tables
                  <span className="normal-case text-slate-400 font-normal border-l border-slate-300 dark:border-slate-700 pl-2 ml-1">{selectedSchemaFilter}</span>
                </h2>
                
                <div className="relative">
                  <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400" size={14} />
                  <input 
                    type="text"
                    placeholder="Search tables..."
                    value={tableSearch}
                    onChange={(e) => setTableSearch(e.target.value)}
                    className="pl-8 pr-3 py-1.5 text-sm bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-700 rounded-md focus:ring-2 focus:ring-indigo-500 outline-none w-full sm:w-48 transition-all"
                  />
                </div>
              </div>
              <span className="text-xs font-medium bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400 px-2.5 py-1 rounded-full border border-indigo-200 dark:border-indigo-800/50 shrink-0 w-fit">
                {addedCount} table{addedCount !== 1 && 's'} added
              </span>
            </div>
            
            <div className="max-h-[200px] overflow-y-auto pr-2 flex flex-wrap gap-2.5 content-start [&::-webkit-scrollbar]:w-1.5 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:bg-slate-300 [&::-webkit-scrollbar-thumb]:dark:bg-slate-600 [&::-webkit-scrollbar-thumb]:rounded-full">
              {filteredTables.length > 0 ? (
                filteredTables.map(tbl => {
                  const key = getTableKey(tbl.schemaName, tbl.tableName);
                  const perm = permissions[key];
                  const isAdded = !!perm;
                  const isSaved = isAdded && perm.isSaved;
                  
                  // Dynamically check if the user has altered the configuration
                  const isModified = perm ? (
                    perm.reason.trim() !== '' || 
                    perm.access_level !== 'unrestricted' || 
                    perm.required_filters.length > 0 || 
                    perm.hasBeenSaved
                  ) : false;
                  
                  let btnClass = 'bg-white text-slate-600 border-slate-200 shadow-sm hover:border-indigo-300 hover:bg-indigo-50 dark:bg-slate-900 dark:text-slate-300 dark:border-slate-700 dark:hover:border-indigo-500 dark:hover:bg-indigo-900/20';
                  if (isModified) {
                    btnClass = 'bg-amber-50 text-amber-700 border-amber-400 shadow-sm hover:bg-amber-100 hover:border-amber-500 dark:bg-amber-900/20 dark:text-amber-400 dark:border-amber-600 dark:hover:border-amber-400';
                  } else if (isAdded) {
                    btnClass = 'bg-indigo-600 text-white border-indigo-600 shadow-sm hover:bg-indigo-700 hover:border-indigo-700';
                  }

                  let tooltip = "Click to add";
                  if (isSaved) tooltip = "Saved! Click to edit.";
                  else if (isModified) tooltip = "Modified! Click to save.";
                  else if (isAdded) tooltip = "Click to remove";

                  return (
                    <button
                      key={tbl.tableName}
                      onClick={() => {
                        if (!isAdded) {
                          handleAddTable(tbl.schemaName, tbl.tableName);
                        } else if (isModified) {
                          if (isSaved) handleEditTable(key);
                          else handleSaveTable(key); // Saves and minimizes it rather than deleting
                        } else {
                          handleRemoveTable(key); // Deletes pristine (untouched) tables
                        }
                      }}
                      className={`flex items-center gap-2 px-3.5 py-2 rounded-full text-sm font-medium transition-all border ${btnClass}`}
                      title={tooltip}
                    >
                      {tbl.tableName}
                      {isModified ? <Check size={14} className="opacity-100" /> : isAdded ? <Check size={14} className="opacity-100" /> : <Plus size={14} className="opacity-40" />}
                    </button>
                  );
                })
              ) : (
                <span className="text-sm text-slate-500 px-3 py-1.5 font-medium">No tables match</span>
              )}
            </div>
          </section>

          {/* Step 4: Configure Permissions */}
          <section className="bg-white dark:bg-slate-900 p-6 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800">
            <h2 className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider mb-4 flex items-center gap-2">
              <span className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Step 4</span> 
              Configure Permissions
            </h2>
            
            <div className={`space-y-4 ${tablesToConfigure.length > 4 ? 'max-h-[600px] overflow-y-auto pr-2 [&::-webkit-scrollbar]:w-1.5 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:bg-slate-300 [&::-webkit-scrollbar-thumb]:dark:bg-slate-600 [&::-webkit-scrollbar-thumb]:rounded-full' : ''}`}>
              {tablesToConfigure.length === 0 ? (
                <div className="border-2 border-dashed border-slate-200 dark:border-slate-800 rounded-xl p-8 text-center flex flex-col items-center justify-center text-slate-400">
                  <TableIcon size={32} className="mb-3 opacity-50" />
                  <p className="text-sm">
                    {addedCount > 0 
                      ? "All added tables have been configured and saved." 
                      : "Add tables from the section above to configure their permissions."}
                  </p>
                </div>
              ) : (
                tablesToConfigure.map(([key, perm]) => {
                  const [schema, tbl] = key.split('.');
                  const isFiltered = perm.access_level === 'filtered';
                  const columns = getColumns(key);

                  return (
                    <div key={key} className="border border-slate-200 dark:border-slate-700 rounded-xl overflow-hidden bg-white dark:bg-slate-900 shadow-sm transition-all hover:shadow-md">
                      {/* Card Header */}
                      <div className="bg-slate-50 dark:bg-slate-800/80 px-4 py-3 flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-slate-200 dark:border-slate-700">
                        <div className="flex items-center gap-2 min-w-0">
                          <TableIcon size={16} className="text-slate-400 shrink-0" />
                          <span className="text-xs px-2 py-0.5 rounded bg-white dark:bg-slate-700 border border-slate-200 dark:border-slate-600 text-slate-500 dark:text-slate-300 font-mono shrink-0">
                            {schema}
                          </span>
                          <span className="text-sm font-semibold truncate">{tbl}</span>
                        </div>
                        
                        <div className="flex items-center gap-2 shrink-0">
                          <div className="flex bg-slate-200 dark:bg-slate-950 p-0.5 rounded-lg border border-slate-300 dark:border-slate-700">
                            {['unrestricted', 'filtered'].map(level => (
                              <button
                                key={level}
                                onClick={() => handleUpdateAccess(key, level)}
                                className={`px-3 py-1 text-xs rounded-md capitalize transition-all ${
                                  perm.access_level === level 
                                    ? level === 'unrestricted' ? 'bg-emerald-500 text-white shadow'
                                    : 'bg-amber-500 text-white shadow'
                                  : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800'
                                }`}
                              >
                                {level}
                              </button>
                            ))}
                          </div>
                          <button 
                            onClick={() => handleRemoveTable(key)}
                            className="p-1.5 text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-md transition-colors"
                            title="Remove table configuration"
                          >
                            <Trash2 size={16} />
                          </button>
                        </div>
                      </div>

                      {/* Card Body */}
                      <div className="p-4 space-y-4">
                        <div className="flex items-start sm:items-center gap-3 flex-col sm:flex-row">
                          <label className="text-xs font-medium text-slate-500 dark:text-slate-400 w-16 shrink-0 flex items-center gap-1">
                            <Info size={14}/> Reason
                          </label>
                          <input 
                            type="text" 
                            placeholder="Why is this access granted or denied?" 
                            value={perm.reason}
                            onChange={(e) => handleUpdateReason(key, e.target.value)}
                            className="flex-1 w-full px-3 py-1.5 text-sm bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-700 rounded focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition-all"
                          />
                        </div>

                        {isFiltered && (
                          <div className="pt-3 border-t border-slate-100 dark:border-slate-800 space-y-3">
                            <div className="text-xs font-semibold text-slate-500 flex items-center gap-1.5">
                              <Filter size={14} /> Row Filters
                            </div>
                            
                            {perm.required_filters.length === 0 ? (
                              <div className="text-sm text-slate-400 italic px-2">No filters added yet. This will behave like 'unrestricted' until filters are defined.</div>
                            ) : (
                              <div className="space-y-2">
                                {perm.required_filters.map((f, fi) => (
                                  <div key={fi} className="flex flex-wrap sm:flex-nowrap items-center gap-2">
                                    <select 
                                      value={f.column}
                                      onChange={(e) => handleUpdateFilter(key, fi, 'column', e.target.value)}
                                      className="flex-1 min-w-[120px] text-sm px-2 py-1.5 bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-600 rounded focus:ring-2 focus:ring-indigo-500 outline-none"
                                    >
                                      {columns.map(c => {
                                        const isUsed = perm.required_filters.some((filter, idx) => filter.column === c && idx !== fi);
                                        return (
                                          <option key={c} value={c} disabled={isUsed}>
                                            {c}
                                          </option>
                                        );
                                      })}
                                    </select>
                                    
                                    <select 
                                      value={f.filter_type}
                                      onChange={(e) => handleUpdateFilter(key, fi, 'filter_type', e.target.value)}
                                      className="w-28 text-sm px-2 py-1.5 bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-600 rounded focus:ring-2 focus:ring-indigo-500 outline-none"
                                    >
                                      {['id', 'enum', 'range', 'text'].map(t => <option key={t} value={t}>{t}</option>)}
                                    </select>
                                    
                                    <input 
                                      type="text" 
                                      placeholder="Value(s), comma-separated" 
                                      value={typeof f.values === 'string' ? f.values : Array.isArray(f.values) ? f.values.join(', ') : ''}
                                      onChange={(e) => handleUpdateFilter(key, fi, 'values', e.target.value)}
                                      className="flex-[2] min-w-[150px] text-sm px-3 py-1.5 bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-600 rounded focus:ring-2 focus:ring-indigo-500 outline-none"
                                    />
                                    
                                    <button 
                                      onClick={() => handleRemoveFilter(key, fi)}
                                      className="p-1.5 text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded transition-colors shrink-0"
                                    >
                                      <X size={16} />
                                    </button>
                                  </div>
                                ))}
                              </div>
                            )}
                            
                            <button 
                              onClick={() => handleAddFilter(key)}
                              disabled={perm.required_filters.length >= columns.length}
                              className={`inline-flex items-center gap-1 text-xs px-3 py-1.5 border border-dashed border-slate-300 dark:border-slate-600 rounded text-slate-600 dark:text-slate-400 transition-colors ${
                                perm.required_filters.length >= columns.length 
                                  ? 'opacity-50 cursor-not-allowed bg-slate-50 dark:bg-slate-800' 
                                  : 'hover:bg-slate-50 dark:hover:bg-slate-800 hover:text-indigo-600 dark:hover:text-indigo-400'
                              }`}
                            >
                              <Plus size={14} /> Add column filter
                            </button>
                          </div>
                        )}
                        
                        <div className="pt-4 mt-2 border-t border-slate-100 dark:border-slate-800 flex justify-end">
                          <button 
                            onClick={() => handleSaveTable(key)}
                            className="flex items-center gap-2 px-4 py-1.5 text-sm font-medium rounded-lg bg-emerald-500 hover:bg-emerald-600 text-white shadow-sm transition-colors"
                          >
                            <Check size={16} /> Save Configuration
                          </button>
                        </div>
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          </section>

          {/* Step 5: JSON Payload */}
          <section className="bg-white dark:bg-slate-900 p-6 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800">
            <h2 className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider mb-4 flex items-center gap-2">
              <span className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">Step 5</span> 
              JSON Payload
            </h2>
            <div className="relative group">
              <pre className="bg-slate-900 dark:bg-[#0d1117] text-slate-50 p-4 rounded-xl font-mono text-xs overflow-x-auto max-h-96 overflow-y-auto border border-slate-800 shadow-inner">
                <code>{JSON.stringify(payload, null, 2)}</code>
              </pre>
              <button 
                onClick={handleCopyPayload}
                className="absolute top-3 right-3 p-2 bg-white/10 hover:bg-white/20 text-white rounded backdrop-blur-sm transition-all opacity-0 group-hover:opacity-100"
                title="Copy to clipboard"
              >
                {copied ? <Check size={16} className="text-green-400"/> : <Copy size={16}/>}
              </button>
            </div>
          </section>

          {/* Footer Actions */}
          <div className="flex items-center justify-end gap-3 pt-4">
            <button 
              onClick={handleCopyPayload}
              className="px-4 py-2 flex items-center gap-2 text-sm font-medium rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 transition-colors"
            >
               {copied ? <Check size={16} className="text-green-500"/> : <Copy size={16}/>}
               {copied ? 'Copied!' : 'Copy JSON'}
            </button>
            <button 
              onClick={handleSendPayload}
              className="px-5 py-2 flex items-center gap-2 text-sm font-medium rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm transition-all"
            >
              <Send size={16} />
              Review & Send
            </button>
          </div>

        </div>
      </div>
    </div>
  );
}