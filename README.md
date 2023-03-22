# Currency strength indicator (CSI)
Ši aplikacija leidžia gyvai stebėti žaliavų bei valiutų santykinius kainų pokyčius su sudarytu instrumentų indeksu. Vietoj to, kad stebėtume vieną ar kelias valiutų poras, šis įrankis leidžia stebėti kainų pokytį su savo nuožiūra sudarytu indeksų. Galimybė, gyvai matyti kas vyksta individualiai su kiekviena valiuta, atveria naują žvilgsnį į forex rinką. Šiuo būdu galima stebėti tiek trendus, tiek rizikingas overbought/oversold teritorijas.

# Naudojimosi taisyklės
1) Aplikacijai veikti būtina Metatrader 4 platforma, nes ji teikia duomenis aplikacijai.
2) Metatrader 4 platformoje reikia paleisti indikatorių (Currency strength indicator.ex5)

# Kaip veikia programa
Nustatymuose ("S" mygtukas) "basket currency" lange galima pridėti valiutų symbolius ir nustatyti jų santykinį svorį w sudarytame portfolio svorio, kur svorių suma lygi Σw. Pvz.:
> Out of: 6 
Skritulinė diagrama vizualiai atvaizduoja kiekvieno instrumento dalį sudarytame indekse.
![alt text](https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/CSI%20currency%20basket.png)

Pagrindiniame lange yra trys zonos: 
1) Centre - kiekvienam instrumentui atskira kortelė, rodanti santykinį kainos pokytį su indeksu. Korteles galima kilnoti, uždaryti atidaryti. Du kartus paspaudus dešinį pelės klavišą ant skaičiaus vaizduojamas grafikas, kairės pelės klavišas atidaro grafiką ant seno kitos valiutos grafiko ir leidžia juos palyginti.
2) Apačioje - zona grafikui, kur x ašis vaizduoja laiką (dešinėje naujausi duomenys), o y ašis rodo valiutos kainos santykinį pokytį procentais bėgant laikui.  
![alt text](https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/CSI%20main%20window.png)
Viduriniame plote galima 
![alt text](https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/CSI%20symbols.png)
