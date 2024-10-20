const apiUrl = "http://localhost:5269/api/Tables";

function addField() {
    const fieldContainer = document.getElementById("fields-container");
    const fieldRow = document.createElement("div");
    fieldRow.classList.add("field-row");

    fieldRow.innerHTML = `
        <input type="text" placeholder="Field Name" class="field-name" />
        <select class="field-type">
            <option value="0">Integer</option>
            <option value="1">Real</option>
            <option value="2">Char</option>
            <option value="3">String</option>
            <option value="4">Date</option>
            <option value="5">Date Interval</option>
        </select>
        <button type="button" onclick="removeField(this)">Remove Field</button>
    `;
    fieldContainer.appendChild(fieldRow);
}

// Remove a field from the table creation form
function removeField(button) {
    button.parentElement.remove();
}

// Create a new table with fields and data types
async function createTable() {
    const tableName = document.getElementById("tableName").value;
    const fields = Array.from(document.querySelectorAll("#fields-container .field-row")).map(row => {
        return {
            name: row.querySelector(".field-name").value,
            type: row.querySelector(".field-type").value // Convert type to integer
        };
    });

    if (!tableName || fields.length === 0) {
        alert("Please provide table name and fields.");
        return;
    }

    const table = {
        name: tableName,
        fields: fields
    };

    try {
        const response = await fetch(apiUrl, {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify(table)
        });

        if (response.ok) {
            alert("Table created successfully!");
            await loadTables();
        } else {
            const errorText = await response.text();
            alert(`Error : ${errorText}`);
        }
    } catch (error) {
        alert("Error creating table.");
    }
}

async function loadTables() {
    const response = await fetch(apiUrl);
    const tables = await response.json();
    const dropdown = document.getElementById("tables-dropdown");
    dropdown.innerHTML = `<option>Select a table</option>` + tables.map(t => `<option value="${t.name}">${t.name}</option>`).join('');
}

async function loadTable(tableName) {
    if (!tableName) return;
    const response = await fetch(`${apiUrl}/${tableName}`);
    const table = await response.json();

    // Show table structure and existing rows
    const tableView = document.getElementById("table-view");
    tableView.innerHTML = `<h3>Table: ${table.name}</h3>`;
    if (table.rows.length === 0) {
        tableView.innerHTML += `<p>No rows available</p>`;
    } else {
        tableView.innerHTML += `<table border="1">
            <thead>
                <tr>${table.fields.map(f => `<th>${f.name}</th>`).join('')}</tr>
            </thead>
            <tbody>
                ${table.rows.map((row, index) => `<tr>${table.fields.map(f => `<td>${row.values[f.name]}</td>`).join('')}<td><button onclick="deleteRow('${table.name}', ${index})">Delete</button><button onclick="editRow('${table.name}', ${index})">Edit</button></td></tr>`).join('')}
            </tbody>
        </table>`;
    }

    // Prepare form for adding a new row
    const rowFields = document.getElementById("row-fields");
    rowFields.innerHTML = table.fields.map(f => `<input type="text" placeholder="${f.name}" id="field-${f.name}" />`).join('');
}

// Add a new row to the selected table
async function addRow() {
    const tableName = document.getElementById("tables-dropdown").value;
    const fieldInputs = Array.from(document.querySelectorAll("#row-fields input"));

    const row = {
        values: fieldInputs.reduce((values, input) => {
            values[input.id.replace("field-", "")] = input.value;
            return values;
        }, {})
    };

    try {
        const response = await fetch(`${apiUrl}/${tableName}/Rows`, {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify(row)
        });


        if (response.ok) {
            alert("Row added successfully!");
            await loadTable(tableName);
        } else {
            const errorText = await response.text();
            alert(`Error : ${errorText}`);
        }
    } catch (error) {
        alert("Error adding row.");
    }
}

// Delete a row from the selected table
async function deleteRow(tableName, index) {
    try {
        const response = await fetch(`${apiUrl}/${tableName}/Rows/${index}`, {method: "DELETE"});

        if (response.ok) {
            alert("Row deleted successfully!");
            await loadTable(tableName);
        } else {
            const errorText = await response.text();
            alert(`Error : ${errorText}`);
        }
    } catch (error) {
        alert("Error deleting row.");
    }
}

// Edit a row by loading its current values into input fields for editing
async function editRow(tableName, index) {
    // Fetch the specific row to edit
    try {
        const response = await fetch(`${apiUrl}/${tableName}/Rows`);
        const rows = await response.json();
        const rowToEdit = rows[index]; // Get the row at the specific index

        if (!rowToEdit) {
            alert("Row not found!");
            return;
        }

        // Populate the row fields with the values from the rowToEdit object
        const rowFieldsContainer = document.getElementById("row-fields");
        rowFieldsContainer.innerHTML = ""; // Clear any previous input fields
        const addRowLabel = document.getElementById("add-row-label");

        for (const [fieldName, value] of Object.entries(rowToEdit.values)) {
            const inputField = document.createElement("input");
            inputField.type = "text";
            inputField.id = `field-${fieldName}`;
            inputField.value = value; // Pre-fill the field with the existing value
            rowFieldsContainer.appendChild(inputField);
        }

        // Add an Update button for submitting the edited row
        const updateButton = document.createElement("button");
        updateButton.textContent = "Update Row";
        updateButton.onclick = async function () {
            await updateRow(tableName, index); // Call the updateRow function
        };
        rowFieldsContainer.appendChild(updateButton);
        addRowLabel.hidden = true;
    } catch (error) {
        alert("Error loading row data.");
    }
}

// Update the row after editing
async function updateRow(tableName, index) {
    const fieldInputs = Array.from(document.querySelectorAll("#row-fields input"));
    const addRowLabel = document.getElementById("add-row-label");
    const updatedRow = {
        values: fieldInputs.reduce((values, input) => {
            values[input.id.replace("field-", "")] = input.value;
            return values;
        }, {})
    };

    try {
        const response = await fetch(`${apiUrl}/${tableName}/Rows/${index}`, {
            method: "PUT",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify(updatedRow)
        });
        if (response.ok) {
            alert("Row updated successfully!");
            addRowLabel.hidden = false;
            await loadTable(tableName);
        } else {
            const errorText = await response.text();
            alert(`Error : ${errorText}`);
        }
    } catch (error) {
        alert("Error updating row.");
    }
}


// Rename a field in the selected table
async function renameField() {
    const tableName = document.getElementById("tables-dropdown").value;
    const oldFieldName = document.getElementById("renameOldField").value;
    const newFieldName = document.getElementById("renameNewField").value;

    if (!tableName || !oldFieldName || !newFieldName) {
        alert("Please provide all values.");
        return;
    }

    const renameRequest = {
        tableName,
        oldFieldName,
        newFieldName
    };

    try {
        const response = await fetch(`${apiUrl}/Rename`, {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify(renameRequest)
        });
        if (response.ok) {
            alert("Field renamed successfully!");
            await loadTable(tableName);
        } else {
            const errorText = await response.text();
            alert(`Error : ${errorText}`);
        }
    } catch (error) {
        alert("Error renaming field.");
    }
}

async function deleteTable() {
    const tableName = document.getElementById("tables-dropdown").value;

    if (!tableName || tableName === "Select a table") {
        alert("Please select a valid table to delete.");
        return;
    }

    const confirmDeletion = confirm(`Are you sure you want to delete the table: ${tableName}?`);
    if (!confirmDeletion) return;

    try {
        const response = await fetch(`${apiUrl}/${tableName}`, {
            method: "DELETE"
        });

        if (response.ok) {
            alert("Table deleted successfully!");
            await loadTables();
        } else {
            const errorText = await response.text();
            alert(`Error deleting table: ${errorText}`);
        }
    } catch (error) {
        alert("Error deleting table.");
    }
}

// Initialize tables on page load
document.addEventListener("DOMContentLoaded", loadTables);
