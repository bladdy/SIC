# Plan — TablesIndex: indicadores de guardado/carga + cierre inmediato de modal + rendimiento

## Diagnóstico

### UX
- `AssignTable()` (`SIC.Frontend/Pages/Tables/TablesIndex.razor.cs:426`): el modal queda abierto durante todo el POST; se cierra hasta recibir respuesta (:446 y :502). Sin spinner ni botón deshabilitado.
- `ReloadDataAsync()` (:86): recarga mesas + invitaciones sin indicador; la pantalla muestra datos viejos congelados hasta que terminan ambas peticiones.

### Rendimiento
1. **Explosión cartesiana**: `GetTablesByCodeAsync` (`SIC.Backend/Repositories/Implemetations/TablesEventsRepository.cs:394`) une en un solo JOIN `TablesEvents × Invitations × Guests × Guests.Invitation` sin `AsSplitQuery()`.
2. **Payload inflado**: `InvitationRepository.GetAllAsync(string code)` (`SIC.Backend/Repositories/Implemetations/InvitationRepository.cs:222`) incluye `Event` completo y nav `TablesEvents` por cada invitación (Event se serializa N veces por `ReferenceHandler.IgnoreCycles`). La página solo usa `Id, Name, Status, Guests, TablesEventsId` (el FK escalar se materializa sin Include).

Único otro consumidor de `api/Tables/tablesbycode`: `TablesStatusForClients.razor.cs:45` (AsSplitQuery es transparente para él). Único consumidor de `api/Invitations/byEventCode`: TablesIndex (:106).

## Cambios

### 1. Frontend — `TablesIndex.razor.cs`
**Campos nuevos** (junto a los flags de modales, ~línea 32):
```csharp
private bool isReloading = false;
private bool isSavingAssignment = false;
private string busyMessage = string.Empty;
private bool IsBusy => isReloading || isSavingAssignment;
```

**`ReloadDataAsync()`**:
```csharp
private async Task ReloadDataAsync()
{
    isReloading = true;
    busyMessage = "Actualizando mesas...";
    StateHasChanged();
    try
    {
        var tablesTask = LoadTablesEventsAsync();
        var invitationsTask = LoadInvitationsAsync();
        await Task.WhenAll(tablesTask, invitationsTask);
        ComputeStats();
    }
    finally
    {
        isReloading = false;
    }
}
```

**`AssignTable()`** — reordenar flujo (ambas ramas):
1. Validaciones de lugares (igual que ahora, antes de cerrar).
2. Construir `dtos`.
3. Cerrar modal de inmediato y activar guardado:
```csharp
modaAsignarMesa = false;
isSavingAssignment = true;
busyMessage = "Guardando asignación...";
StateHasChanged();
try
{
    var response = await Repository.PostAsync<...>(...);
    // manejo de error/éxito idéntico al actual (SweetAlert/toast)
}
finally
{
    isSavingAssignment = false;
}

AssignTablesDto = new();
await ReloadDataAsync();
```
Nota: eliminar las líneas duplicadas `modaAsignarMesa = false;` que hoy están después del POST (:446, :502).

### 2. Frontend — `TablesIndex.razor`
- Envolver el grid de tarjetas (`<div class="row g-4">`, línea 110) en `<div class="position-relative">` y tras él renderizar overlay cuando `IsBusy`:
```razor
@if (IsBusy)
{
    <div class="position-absolute top-0 start-0 w-100 h-100 d-flex justify-content-center align-items-center"
         style="background:rgba(255,255,255,.7); z-index:1050; border-radius:15px;">
        <div class="text-center">
            <div class="spinner-border text-custom mb-2" role="status"></div>
            <div class="text-muted">@busyMessage</div>
        </div>
    </div>
}
```
- Deshabilitar botones PDF / Mesa / Mesas del encabezado con `disabled="@IsBusy"`.

### 3. Backend — `TablesEventsRepository.GetTablesByCodeAsync` (:394)
Agregar `.AsSplitQuery()` antes de `.ToListAsync()`.

### 4. Backend — `InvitationRepository.GetAllAsync(string code)` (:222)
Quitar `.Include(i => i.Event)` y `.Include(i => i.TablesEvents)`.
(Verificado: TablesIndex usa `inv.TablesEventsId` escalar en :56; solo la línea :46 usa la nav.)

### 5. Frontend — ajustar filtro por FK (TablesIndex.razor.cs:46)
`i.TablesEvents == null` → `i.TablesEventsId == null`.

## Verificación
- `dotnet build SIC.sln` (única verificación disponible; no hay tests ni linter).
