<script>
  import {onMount} from 'svelte';
  import AmmoLib from 'three/examples/js/libs/ammo.wasm';
  import { CarSimulation } from './ts/CarSimulation.ts'
  
  // Properties
  export let title;
  
  let carSimulation;

  onMount(() => {
    AmmoLib().then(function (ammoLib) {
      Ammo = ammoLib;
      carSimulation = new CarSimulation(document.getElementById('container'), Ammo);
    });

    // add interval for svelte reactivity (update property for reactivity in object)
    const interval = setInterval(() => {
      carSimulation.speed = carSimulation.speed;
    }, 10);

    return () => { clearInterval(interval); };
  });

  const precisionRound = (x, round) => (Math.round(x*10**round)/10**round).toFixed(round)
  $: speedText = `${precisionRound(carSimulation ? carSimulation.speed: 0,2)} km/h`;
  $:  if (carSimulation && carSimulation.speed >= 60) {
	      console.log(`Wow ! ${speedText}`);
      }

</script>

<style>
  h1 {
    color: lightblue;
  }
</style>
<div class="center">
  <div>
    <h1>{title}</h1>
    {#if carSimulation}
      <p>  {speedText} </p>
    {/if}
  </div>
</div>
<div id="container"></div>
