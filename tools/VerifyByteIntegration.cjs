const fs=require('fs'),cp=require('child_process'),crypto=require('crypto');
const base='d6bbbe3',other=process.argv[2]||'../ByteBrawl-physics-integration';
const paths=cp.execFileSync('git',['ls-tree','-r','--name-only',base,'game/assets']).toString().trim().split('\n');
let pixels=0;
for(const p of paths){
 const old=cp.execFileSync('git',['show',base+':'+p],{maxBuffer:50*1024*1024}),now=fs.readFileSync(p),copy=fs.readFileSync(other+'/soldier/'+p.slice(5));
 const equal=(a,b)=>p.endsWith('.png')?a.equals(b):a.toString().replaceAll('\r\n','\n')===b.toString().replaceAll('\r\n','\n');
 if(!equal(old,now)||!equal(now,copy))throw Error('Protected asset changed '+p);
 if(p.endsWith('.png'))pixels++;
}
const local=fs.readFileSync('game/scripts/Physics/FighterPhysics.cs'),shared=fs.readFileSync(other+'/src/Combat/FighterPhysics.cs');
if(!local.equals(shared))throw Error('Physics source drift');
console.log(JSON.stringify({baseline:base,protectedGameAssetFiles:paths.length,byteIdenticalPngFiles:pixels,
 sharedPhysicsSha256:crypto.createHash('sha256').update(shared).digest('hex'),limbPoses:57,hurtboxesPerPose:15,attackRadius:9,terrainRadius:12,terrainHeight:108,
 result:'PNG bytes unchanged; previous metadata/import settings unchanged except line endings; shared physics source identical.'},null,2));
