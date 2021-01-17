<script>
  import {onMount} from 'svelte';
  import AmmoLib from 'three/examples/js/libs/ammo.wasm';
  import { Simulation } from './ts/Simulation.ts'
  
  export let title;
  let count = 0;
  onMount(() => {
    
    // console.log('Ammo', AmmoLib);
    AmmoLib().then(function (ammoLib) {
      
      // console.log('AmmoLib', ammoLib);
      Ammo = ammoLib;
      const simulation = new Simulation(document.getElementById('container'), Ammo);

    });

    const interval = setInterval(() => count++, 1000);
    return () => {
      clearInterval(interval);
    };
  });
</script>

<style>
  h1 {
    color: lightblue;
  }
</style>
<div class="center">
  <div>
    <h1>{title}</h1>
    <p>Page has been open for {count} seconds.</p>
  </div>
</div>
<div id="container"></div>
