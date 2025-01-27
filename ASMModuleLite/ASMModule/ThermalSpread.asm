.data
.code
_DllMainCRTStartup proc parameter1:DWORD, parameter2:DWORD, parameter3:DWORD  ;entry point
    mov eax, 1
    ret
_DllMainCRTStartup endp

;constants
TRUE EQU 1
FALSE EQU 0

;registers aliases - params
InitialReadDataAddress EQU RCX			;RCX
WriteDataAddress EQU RDX				;RDX
WidthPx EQU R8							;R8
HeightPx EQU R9							;R9

;register variables

Shift8 EQU CL

RowsLoopCounter EQU R10
ColumnsLoopCounter EQU R11

Sum EQU R12
Sum32 EQU R12d
Sum8 EQU R12b

FalseRegister EQU R13

General EQU R14
General8 EQU R14b

ReadDataAddress EQU R15				;RDX

IsFinished EQU RAX	

_runSingleStep proc EXPORT USES Sum General FalseRegister ReadDataAddress, readData_:PTR BYTE, writeData_:PTR BYTE, WidthPx_:DWORD, HeightPx_:DWORD, StartColumn:QWORD, EndColumn:QWORD, Multiplicator: DWORD, _shift: DWORD
    mov ReadDataAddress, InitialReadDataAddress
    xor General, General
    mov Shift8, byte ptr _shift

    mov FalseRegister, FALSE
    mov IsFinished, TRUE ; Flaga ustawiona na TRUE na pocz�tku

    mov RowsLoopCounter, 1
rows_loop:
    add ReadDataAddress, WidthPx                ; baseAddress += WidthPx
    add WriteDataAddress, WidthPx               ; baseAddress += WidthPx

    mov ColumnsLoopCounter, StartColumn
    mov IsFinished, TRUE ; Flaga ustawiana na TRUE na pocz�tku ka�dej iteracji wiersza
    columns_loop:
        ; Wczytaj 32 bajty z pami�ci do rejestru wektorowego (AVX - 256-bit)
        vmovdqu ymm0, ymmword ptr [ReadDataAddress + ColumnsLoopCounter]

             vpcmpeqb ymm0, ymm0, ymm0        ; Por�wnaj wszystkie bajty w ymm0 z zerem
        vptest  ymm0, ymm0                ; Sprawd�, czy wszystkie bajty w ymm0 s� zerowe
        jz      data_invalid              ; Skocz, je�li wszystkie bajty s� zerowe (dane s� nieprawid�owe)

        ; Je�li dane s� prawid�owe, kontynuuj:
        jne     data_valid

data_invalid:
        mov     IsFinished, FALSE         ; Ustaw flag� na FALSE, je�li dane s� nieprawid�owe
        jmp     end_column                ; Przerwij przetwarzanie kolumny

    data_valid:

        ; Dodaj s�siad�w (-1, +1 w poziomie)
        vpslld ymm1, ymm0, 8                    ; Przesuni�cie o 1 w lewo
        vpsrld ymm2, ymm0, 8                    ; Przesuni�cie o 1 w prawo
        vpaddb ymm3, ymm1, ymm2                 ; Dodaj s�siad�w poziomych

        ; Dodaj s�siad�w (-1, +1 w pionie)
        mov rax, ReadDataAddress
        add rax, ColumnsLoopCounter
        sub rax, WidthPx
        vmovdqu ymm1, ymmword ptr [rax]
        mov rax, ReadDataAddress            ; Za�aduj ReadDataAddress do rax
        add rax, ColumnsLoopCounter         ; Dodaj ColumnsLoopCounter do rax
        add rax, WidthPx                    ; Dodaj WidthPx do rax
        vmovdqu ymm2, ymmword ptr [rax]     ; Za�aduj dane z obliczonego adresu do ymm2
        vpaddb ymm3, ymm3, ymm1
        vpaddb ymm3, ymm3, ymm2

        ; Za�aduj warto�� Shift8 do rejestru wektorowego
        mov al, Shift8         ; Za�aduj 8-bitow� warto�� z Shift8 do rejestru al
        movd xmm2, eax         ; Przenie� 32-bitow� warto�� z eax do xmm2
        ; Mno�enie i przesuni�cie
        vpmulld ymm3, ymm3, Multiplicator       ; Mno�enie przez sta��
        vpsrld ymm3, ymm3, xmm2                 ; Dzielenie przez przesuni�cie

        ; Zapisz wynik
        vmovdqu ymmword ptr [WriteDataAddress + ColumnsLoopCounter], ymm3

        ; Aktualizacja licznika kolumn
        add ColumnsLoopCounter, 32              ; Przeskok o 32 bajty
        cmp ColumnsLoopCounter, EndColumn
        jna columns_loop
    end_column:

    inc RowsLoopCounter
    cmp RowsLoopCounter, HeightPx
    jna rows_loop

    ret
_runSingleStep endp



END
