const fs=require('fs'),cp=require('child_process'),assert=require('assert');
const {PNG}=require(require('path').join(require('os').homedir(),'.codex/tools/code-as-pixel-art/node_modules/pngjs'));
const base='0abfc77'; const old=p=>JSON.parse(cp.execFileSync('git',['show',base+':'+p],{maxBuffer:100*1024*1024}));
const current=p=>JSON.parse(fs.readFileSync(p));
const a=old('art/soldier.pixel.json'),b=current('art/soldier.pixel.json');
assert.deepStrictEqual(a.palette,b.palette);const ac=a.characters[0],bc=b.characters[0];
for(const key of ['parts','layers','width','height','origin','pivot'])assert.deepStrictEqual(ac[key],bc[key]);
for(const f of ac.views[0].frames)assert.deepStrictEqual(f,bc.views[0].frames.find(g=>g.id===f.id));
for(const c of ac.animations)assert.deepStrictEqual(c,bc.animations.find(g=>g.id===c.id));
const poses=current('art/poses.json').frames,oldposes=old('art/poses.json').frames;assert.equal(poses.length,57);assert.deepStrictEqual(poses.slice(0,45),oldposes);
assert.deepStrictEqual(current('art/thruster-sockets.json').slice(0,45),old('art/thruster-sockets.json'));
assert.equal(current('art/thruster-sockets.json').length,57);
for(const pose of poses.slice(45)){
 assert.deepStrictEqual(pose.parts.near_foot,poses[0].parts.near_foot);
 const frame=bc.views[0].frames.find(f=>f.id===pose.id);
 for(const cel of Object.values(frame.cels))for(let i=0;i<128;i++)assert(!cel.grid.cells[i]&&!cel.grid.cells[127*128+i]&&!cel.grid.cells[i*128]&&!cel.grid.cells[i*128+127],pose.id+' clipped');
}
const fx=current('art/kick-effects.pixel.json').characters[0];
assert.deepStrictEqual(fx.animations[0].frames,bc.animations.find(a=>a.id==='kick').frames);
assert.equal(fx.animations[0].frames.reduce((n,f)=>n+f.durationTicks,0),90);
const kick=PNG.sync.read(fs.readFileSync('art/exports/effects/kick.png'));
for(const dest of ['art/pixelloid/effects/kick.png','game/assets/soldier_effects/kick.png'])assert(kick.data.equals(PNG.sync.read(fs.readFileSync(dest)).data));
for(const clip of ac.animations)cp.execFileSync('git',['diff','--exit-code',base,'--','art/exports/animations/'+clip.id+'.png','art/exports/animations/'+clip.id+'.json']);
let maxError=0;
for(const p of poses.slice(45))for(const [id,bone] of Object.entries(p.parts)){
 const ref=poses[0].parts[id];if(!ref.End)continue;
 const len=b=>Math.hypot(b.End.X-b.Start.X,b.End.Y-b.Start.Y);
 const err=Math.abs(len(ref)-len(bone));maxError=Math.max(maxError,err);assert(err<1.42,id);
}
for(const name of fs.readdirSync('art/exports/layers').filter(n=>n.endsWith('.png'))){
 const p='art/exports/layers/'+name,o=PNG.sync.read(cp.execFileSync('git',['show',base+':'+p])),n=PNG.sync.read(fs.readFileSync(p));
 assert.equal(n.width,57*128);for(let y=0;y<128;y++)assert(o.data.subarray(y*o.width*4,(y+1)*o.width*4).equals(n.data.subarray(y*n.width*4,y*n.width*4+o.width*4)),name);
 for(const dest of ['art/pixelloid/layers/','game/assets/soldier_frames/'])assert(n.data.equals(PNG.sync.read(fs.readFileSync(dest+name)).data));
}
cp.execFileSync('git',['diff','--exit-code',base,'--','art/draw','art/parts','art/palette.json','art/stages','game/assets/stages','game/assets/stage','art/exports/stages','art/fight-effects.pixel.json','art/cross-effects.pixel.json','art/boot-thrusters.pixel.json','art/pack-thrusters.pixel.json','art/exports/effects/attack.png','art/exports/effects/cross.png']);
for(const kind of ['boot','pack']){const p=PNG.sync.read(fs.readFileSync('art/exports/effects/'+kind+'-thrusters.png'));assert.equal(p.width,64);assert(p.data.equals(PNG.sync.read(fs.readFileSync('game/assets/soldier_effects/'+kind+'-thrusters.png')).data));}
console.log(JSON.stringify({protectedPartFrames:720,bodyAtlasFrames:57,maxBoneLengthRoundingError:maxError,oldClipsUnchanged:true,rgbaIdentity:true},null,2));
