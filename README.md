# Currency strength indicator (CSI)
Ši aplikacija leidžia gyvai stebėti žaliavų bei valiutų procentinius kainų pokyčius palyginus su sudarytu instrumentų indeksu. Vietoj to, kad stebėtume vieną ar kelias valiutų poras, šis įrankis leidžia stebėti kainų pokytį su savo nuožiūra sudarytu indeksų. Galimybė, gyvai matyti kas vyksta individualiai su kiekviena valiuta, atveria naują žvilgsnį į forex rinką. Šiuo būdu galima stebėti tiek trendus, tiek rizikingas overbought/oversold teritorijas.

# Naudojimosi taisyklės
1) Aplikacijai veikti būtina Metatrader platforma, nes ji teikia duomenis aplikacijai.
2) Metatrader platformoje reikia paleisti indikatorių (C#_handle, skirtingi failai 4 ir 5 versijoms)

# C#_handle sąsaja su Metatrader platforma
mql kodas Metatrader platformoje irašo nurodytus symbolių duomenis į atskirus *\*.csv* failus ir nuolatos atnaujina informaciją.
![alt text](https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/data%20files.png)
<br><br>faile įrašomos vieno candlestick kainos *Low High Open Close* ir data.
<br><img src="https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/eurjpy.png" alt="" data-canonical-src="https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/eurjpy.png" width="25%" height="auto"/>
# Kaip veikia programa
Nustatymuose ("S" mygtukas) "basket currency" lange galima pridėti valiutų symbolius ir nustatyti jų svorį **w<sub>i</sub>** sudarytame portfolio, kur svorių suma lygi **Σw<sub>i</sub>** Pvz.:
> Out of: 6 

Skritulinė diagrama vizualiai atvaizduoja kiekvieno instrumento dalį sudarytame indekse.
![alt text](https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/CSI%20currency%20basket.png)
<br><br><br>Pagrindiniame lange yra trys zonos:
![alt text](https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/CSI%20main%20window.png)
1) Centre - kiekvienam instrumentui atskira kortelė, rodanti santykinį kainos pokytį su indeksu. Korteles galima kilnoti, uždaryti atidaryti. Du kartus paspaudus dešinį pelės klavišą ant skaičiaus vaizduojamas grafikas, kairės pelės klavišas atidaro grafiką ant seno kitos valiutos grafiko ir leidžia juos palyginti.
2) Apačioje - zona grafikui, kur x ašis vaizduoja laiką (dešinėje naujausi duomenys), o y ašis rodo valiutos kainos santykinį pokytį procentais bėgant laikui. Grafike taip pat vaizduojamas eksponentinis vidurkis.
3) Dešinėje - nustatymų mygtukas ir teksto langas sukurti centre naujai instrumento kortelei.

Norint naudotis valiutas ar žaliavas, pirma reikia prideti jų symbolius įkainotus amerikos doleriais. 
![alt text](https://github.com/TadasDanilevicius/Currency-strength-indicator/blob/main/CSI%20symbols.png)

