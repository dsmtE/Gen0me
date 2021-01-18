<script>
  import {onMount} from 'svelte';
  import AmmoLib from 'three/examples/js/libs/ammo.wasm';
  import { CarSimulation } from './ts/CarSimulation.ts'
  
  // Properties
  export let title;
  let count = 0;
  
  let carSimulation;

  onMount(() => {
    AmmoLib().then(function (ammoLib) {
      Ammo = ammoLib;
      carSimulation = new CarSimulation(document.getElementById('container'), Ammo);
    });
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
