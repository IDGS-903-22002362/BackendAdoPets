# ?? SOLUCIÓN RÁPIDA: No puedo acceder desde mi celular

## ? Problema
```
http://192.168.100.11:5151/api/v1 no accesible desde Android
```

## ? Solución en 3 Pasos

---

### **Paso 1: Modificar launchSettings.json**

**Ubicación:** `AdoPetsBKD/Properties/launchSettings.json`

**Cambiar de:**
```json
"applicationUrl": "http://localhost:5151"
```

**A:**
```json
"applicationUrl": "http://0.0.0.0:5151"
```

**Archivo completo:**

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://0.0.0.0:5151",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://0.0.0.0:7072;http://0.0.0.0:5151",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**O copiar el archivo ya corregido:**
```powershell
cd C:\Users\dell\source\repos\AdoPetsBKD\AdoPetsBKD\Properties
copy launchSettings.json.new launchSettings.json
```

---

### **Paso 2: Configurar el Firewall de Windows**

**Abrir PowerShell como Administrador** y ejecutar:

```powershell
# Permitir conexiones entrantes al puerto 5151
New-NetFirewallRule -DisplayName "AdoPets Backend HTTP" -Direction Inbound -Protocol TCP -LocalPort 5151 -Action Allow

# Permitir conexiones entrantes al puerto 7072 (HTTPS, opcional)
New-NetFirewallRule -DisplayName "AdoPets Backend HTTPS" -Direction Inbound -Protocol TCP -LocalPort 7072 -Action Allow
```

**Verificar que las reglas se crearon:**
```powershell
Get-NetFirewallRule -DisplayName "AdoPets Backend*" | Select-Object DisplayName, Enabled, Direction, Action
```

**Salida esperada:**
```
DisplayName               Enabled Direction Action
-----------               ------- --------- ------
AdoPets Backend HTTP      True    Inbound   Allow
AdoPets Backend HTTPS     True    Inbound   Allow
```

**Método alternativo (GUI):**

1. Presiona `Windows + R` ? escribe `wf.msc` ? Enter
2. **Reglas de entrada** ? **Nueva regla...**
3. Tipo de regla: **Puerto** ? Siguiente
4. TCP, puertos específicos: **5151** ? Siguiente
5. **Permitir la conexión** ? Siguiente
6. Aplicar a **Dominio, Privado y Público** ? Siguiente
7. Nombre: **AdoPets Backend HTTP** ? Finalizar

---

### **Paso 3: Ejecutar el Backend**

```powershell
cd C:\Users\dell\source\repos\AdoPetsBKD\AdoPetsBKD

# Ejecutar
dotnet run
```

**Verificar en los logs:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5151
      Application started. Press Ctrl+C to shut down.
```

? **SI dice `0.0.0.0:5151`** ? ¡Perfecto!
? **SI dice `localhost:5151`** ? Vuelve al Paso 1

---

## ?? Verificación

### Prueba 1: Desde tu PC

Abre tu navegador en la PC y ve a:
```
http://192.168.100.11:5151/swagger
```

**Resultado esperado:** Página de Swagger carga ?

### Prueba 2: Desde tu Celular

1. Asegúrate de que tu celular esté en la **misma WiFi** que tu PC
2. Abre el navegador en tu Android
3. Ve a: `http://192.168.100.11:5151/swagger`

**Resultado esperado:** Página de Swagger carga ?

---

## ?? Si Aún No Funciona

### Verificar IP de tu PC

```powershell
ipconfig
```

Busca tu IP en la sección **Adaptador de LAN inalámbrica Wi-Fi**:
```
Adaptador de LAN inalámbrica Wi-Fi:
   Dirección IPv4. . . . . . . . : 192.168.100.11
```

**Si la IP cambió:**
- Actualiza en tu app Flutter
- Usa la nueva IP

### Verificar que el Puerto no esté Bloqueado

```powershell
# Ver qué está escuchando en el puerto 5151
netstat -ano | findstr :5151
```

**Salida esperada:**
```
TCP    0.0.0.0:5151          0.0.0.0:0              LISTENING       12345
```

- `0.0.0.0:5151` ? ? Escuchando en todas las interfaces
- `127.0.0.1:5151` ? ? Solo en localhost

### Verificar Firewall

```powershell
# Ver reglas de firewall para el puerto 5151
Get-NetFirewallPortFilter | Where-Object LocalPort -eq 5151 | Get-NetFirewallRule
```

**Debe mostrar las reglas que creaste.**

### Deshabilitar Temporalmente el Firewall (Solo para probar)

**?? Solo para diagnóstico, vuelve a habilitarlo después:**

```powershell
# Deshabilitar firewall (como Admin)
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled False

# Probar conexión desde el celular

# Habilitar firewall de nuevo (IMPORTANTE)
Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled True
```

Si funciona con el firewall deshabilitado, el problema es la configuración del firewall.

### Verificar que PC y Celular están en la misma red

**En tu PC:**
```powershell
ipconfig | findstr "IPv4"
```
Ejemplo: `192.168.100.11`

**En tu celular:**
- Ajustes ? WiFi ? Red conectada ? Ver detalles
- Dirección IP debe empezar con `192.168.100.xxx`

Si tienen diferentes rangos (ej: PC en `192.168.1.x` y celular en `192.168.100.x`), están en redes diferentes.

---

## ?? Checklist Final

- [ ] `launchSettings.json` tiene `0.0.0.0:5151` en lugar de `localhost:5151`
- [ ] Firewall permite conexiones al puerto 5151
- [ ] Backend ejecutándose con `dotnet run`
- [ ] Logs muestran: `Now listening on: http://0.0.0.0:5151`
- [ ] `http://192.168.100.11:5151/swagger` abre desde navegador de PC
- [ ] PC y celular en la misma red WiFi (192.168.100.x)
- [ ] `http://192.168.100.11:5151/swagger` abre desde navegador de celular

---

## ? Solución Rápida sin Modificar Archivos

Si no quieres modificar `launchSettings.json`, puedes ejecutar así:

```powershell
dotnet run --urls "http://0.0.0.0:5151"
```

Esto sobrescribe la configuración de `launchSettings.json` temporalmente.

---

## ?? Resumen

El problema era que el backend solo escuchaba en `localhost` (127.0.0.1), que solo es accesible desde la misma PC.

Al cambiar a `0.0.0.0`, el servidor escucha en **todas las interfaces de red**, permitiendo conexiones desde:
- ? localhost (tu PC)
- ? 192.168.100.11 (desde otros dispositivos en tu red)
- ? Tu celular Android

---

## ?? Siguiente Paso

Una vez que puedas acceder a `http://192.168.100.11:5151/swagger` desde tu celular, estarás listo para probar Firebase Authentication en la app Flutter.
