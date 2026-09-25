// Derive capsule geometry from approved pixel-source cells; never changes sprite pixels.
const fs=require('fs');
const source=JSON.parse(fs.readFileSync('art/soldier.pixel.json')).characters[0];
const poses=JSON.parse(fs.readFileSync('art/poses.json')).frames;
const frames=new Map(source.views[0].frames.map(f=>[f.id,f]));
const parts=source.layers.map(l=>l.id).filter(id=>id!=='backpack');
const output=poses.map(p=>({id:p.id,parts:parts.map(id=>{
 const points=frames.get(p.id).cels[id].grid.cells.flatMap((v,i)=>v?[{x:i%128,y:Math.floor(i/128)}]:[]);
 const bone=p.parts[id];let dx=0,dy=1;
 if(bone.End){dx=bone.End.X-bone.Start.X;dy=bone.End.Y-bone.Start.Y;const len=Math.hypot(dx,dy);dx/=len;dy/=len;}
 else {const w=Math.max(...points.map(p=>p.x))-Math.min(...points.map(p=>p.x)),h=Math.max(...points.map(p=>p.y))-Math.min(...points.map(p=>p.y));if(w>h){dx=1;dy=0;}}
 const along=points.map(p=>p.x*dx+p.y*dy),across=points.map(p=>-p.x*dy+p.y*dx);
 const lo=Math.min(...along),hi=Math.max(...along),left=Math.min(...across),right=Math.max(...across);
 const radius=(right-left+1)/2,height=Math.max(2*radius,hi-lo+1+radius*.6);
 const a=(lo+hi)/2,b=(left+right)/2;
 return {id,x:a*dx-b*dy-64,y:a*dy+b*dx-121,radius,height,rotation:Math.atan2(dy,dx)-Math.PI/2,
   group:id==='head'?'Head':/arm|hand/.test(id)?'Arm':/thigh|shin|foot/.test(id)?'Leg':'Torso'};
})}));
fs.writeFileSync('game/assets/soldier_frames/hurtboxes.json',JSON.stringify({source:'ByteBrawl LimbRig capsule rule fitted to approved Soldier pixels',frames:output},null,2)+'\n');
console.log(`Generated ${output.length} poses × ${parts.length} capsules; thigh/shin included; no raster changes.`);
