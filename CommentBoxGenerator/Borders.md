Borders


2. abschnitt bit 
3: 0=Light, 1=Heavy
2: 0=Horizontal, 2=Vertikal
0+1: 01 = TripleDash, 10 = Quad Dash


50-25F|0-F

Vertical
*.0000 |0000    L-
*.0000 |0001    H-
*.0000 |0010    L|
*.0000 |0011    H|
*.0000 |0100    L3- => dashed? setz das 3. letzt bit
*.0000 |0101    H3-
*.0000 |0110    L3| => 4er-Dash? setz auch das 4. letzte bit
*.0000 |0111    H3|
*.0000 |1000    L4- => dashed? setz das 3. letzt bit
*.0000 |1001    H4-
*.0000 |1010    L4| => 4er-Dash? setz auch das 4. letzte bit
*.0000 |1011    H4|
------  Edge  -----------------
Down-Right - xxx0 |11xx
*.0000 |1100    LD LR-> 0=light, 1=heavy
*.0000 |1101    LD HR
*.0000 |1110    HD LR
*.0000 |1111    HD HR
Down-Left - xxx1 |00xx
*.0001 |0000    LD LL
*.0001 |0001    LD HL
*.0001 |0010    HD LL
*.0001 |0011    HD HL
Up-Right - xxx1 |01xx
*.0001 |0100    LU LR
*.0001 |0101    LU HR
*.0001 |0110    HU LR
*.0001 |0111    HU HR
Up-Left - xxx1 |10xx
*.0001 |1000    LU LL
*.0001 |1001    LU HL
*.0001 |1010    HU LL
*.0001 |1011    HU HL

Corners

1 => Heavy, 0=> light
0250.C-F    Down-Right
LD LR ┌     +00
LD HR ┍     +01
HD LR ┎     +10
HD HR ┏     +11

0251.0-3    Down-Left

0251.4-7    Up-Right

0251.8-B    Up-Left
