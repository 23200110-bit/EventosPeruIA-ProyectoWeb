"use strict";
const form=document.querySelector("#auth-form"),message=document.querySelector("#message"),submit=document.querySelector("#submit");
let mode="register";const params=new URLSearchParams(location.search);let resetToken=params.get("token");
function switchMode(next){mode=next;message.textContent="";form.hidden=false;document.querySelector("#session").hidden=true;
 document.querySelectorAll("[data-mode]").forEach(b=>b.classList.toggle("active",b.dataset.mode===mode));
 const registration=mode==="register",password=mode!=="forgot",confirmation=registration||mode==="reset";
 document.querySelector("#names").hidden=!registration;["nombres","apellidos"].forEach(n=>form.elements[n].disabled=!registration);
 document.querySelector("#password-label").hidden=!password;form.elements.contrasena.disabled=!password;form.elements.contrasena.minLength=mode==="login"?1:8;form.elements.contrasena.autocomplete=mode==="login"?"current-password":"new-password";
 document.querySelector("#confirm-label").hidden=!confirmation;form.elements.confirmarContrasena.disabled=!confirmation;document.querySelector("#interests").hidden=!registration;
 document.querySelector("#form-title").textContent=({register:"Haz espacio para algo nuevo.",login:"Qué bueno verte de nuevo.",forgot:"Recupera tu acceso.",reset:"Elige tu nueva contraseña."})[mode];
 document.querySelector("#subtitle").textContent=mode==="forgot"?"Te enviaremos instrucciones si tu cuenta está activa.":mode==="reset"?"Tu enlace es válido por 30 minutos y un solo uso.":"Una cuenta para participar y organizar.";
 submit.textContent=({register:"Crear mi cuenta",login:"Ingresar",forgot:"Solicitar recuperación",reset:"Guardar nueva contraseña"})[mode];validate();}
function validate(){const a=form.elements.contrasena,b=form.elements.confirmarContrasena;b.setCustomValidity(!b.disabled&&b.value!==a.value?"Las contraseñas no coinciden.":"");submit.disabled=!form.checkValidity();}
form.addEventListener("input",validate);document.querySelectorAll("[data-mode]").forEach(b=>b.addEventListener("click",()=>switchMode(b.dataset.mode)));document.querySelector("#forgot").onclick=()=>switchMode("forgot");
form.addEventListener("submit",async e=>{e.preventDefault();if(!form.reportValidity())return;submit.disabled=true;message.textContent="Procesando…";
 const data=Object.fromEntries(new FormData(form));if(mode==="register")data.interesesIds=[...document.querySelectorAll("#categories input:checked")].map(x=>Number(x.value));if(mode==="reset")data.token=resetToken;
 try{const endpoint=({register:"register",login:"login",forgot:"forgot-password",reset:"reset-password"})[mode];const r=await fetch(`/api/auth/${endpoint}`,{method:"POST",headers:{"Content-Type":"application/json"},body:JSON.stringify(data)});const body=await r.json().catch(()=>({}));if(!r.ok)throw new Error(body.title||Object.values(body.errors||{}).flat().join(" ")||`Error ${r.status}`);
 if(mode==="login"){sessionStorage.setItem("eventos.jwt",body.token);form.hidden=true;document.querySelector("#session").hidden=false;document.querySelector("#welcome").textContent=`Bienvenido/a, ${body.usuario.nombres}. Sesión iniciada.`;message.textContent="";}
 else if(mode==="register"){switchMode("login");message.textContent="Cuenta creada. Ya puedes ingresar.";}
 else{message.textContent=body.mensaje;if(mode==="reset"){resetToken=null;history.replaceState(null,"",location.pathname);switchMode("login");message.textContent="Contraseña actualizada. Ingresa con la nueva.";}}
 }catch(err){message.textContent=err.message||"No se pudo conectar con la API.";}finally{validate();}});
document.querySelector("#logout").onclick=()=>{sessionStorage.removeItem("eventos.jwt");switchMode("login");};
fetch("/api/categorias").then(r=>{if(!r.ok)throw Error();return r.json();}).then(rows=>{for(const x of rows){const label=document.createElement("label"),input=document.createElement("input");input.type="checkbox";input.value=x.categoriaId;label.append(input,document.createTextNode(" "+x.nombre));document.querySelector("#categories").append(label);}}).catch(()=>{document.querySelector("#categories").textContent="Intereses no disponibles: comprueba la conexión con SQL Server.";});
if(resetToken){form.elements.correo.value=params.get("correo")||"";history.replaceState(null,"",location.pathname);switchMode("reset");}else switchMode("register");
