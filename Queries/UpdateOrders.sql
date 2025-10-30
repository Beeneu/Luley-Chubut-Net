SELECT
  T0."U_PO_CHUBUT" AS "numPedido",
  T0."U_SUB_PO_CHUBUT" AS "numSubPedido", 
  COALESCE(T7."U_CodigoInterfaz", T1."ItemCode") AS "codArticulo",
  T3."PTICode" || '-' || LPAD(T3."FolNumFrom", 8, '0') AS "nroRemito",
  T2."Quantity" AS "cantidadRemitida",
  CASE
    WHEN COALESCE(T5."DocEntry", T6."DocEntry") IS NOT NULL OR (T3."DocNum" IS NULL AND T0."DocStatus" = 'C') THEN 7
    WHEN COALESCE(T4."DocEntry", T5."DocEntry") IS NOT NULL THEN 6
    WHEN T3."DocNum" IS NOT NULL AND T3."U_EstadoRemito" = 'DIS' THEN 5
    WHEN T3."DocNum" IS NOT NULL AND T3."U_ESTADO_LOGISTICA" IN (2, 3) THEN 4
    WHEN T3."DocNum" IS NOT NULL AND T3."U_ESTADO_LOGISTICA" = 1 THEN 3
    WHEN T3."DocNum" IS NOT NULL AND T3."U_ESTADO_LOGISTICA" = 0 THEN 2
    WHEN T3."DocNum" IS NULL THEN 1
    ELSE -1
  END AS "estadoRemito"


FROM ORDR T0
INNER JOIN RDR1 T1 ON T1."DocEntry" = T0."DocEntry"
INNER JOIN OITM T7 ON T7."ItemCode" = T1."ItemCode"
LEFT JOIN DLN1 T2 ON T2."BaseType" = 17 AND T2."BaseEntry" = T1."DocEntry" AND T2."BaseLine" = T1."LineNum"
LEFT JOIN ODLN T3 ON T2."DocEntry" = T3."DocEntry"
LEFT JOIN RRR1 T4 ON T4."BaseEntry" = T2."DocEntry" AND T4."BaseType" = 15 AND T4."BaseLine" = T2."LineNum" 
LEFT JOIN RDN1 T5 ON T5."BaseEntry" = T4."DocEntry" AND T5."BaseType" = 234000031 AND T5."BaseLine" = T4."LineNum"
LEFT JOIN RDN1 T6 ON T6."BaseEntry" = T2."DocEntry"  AND T6."BaseType" = 15 AND T6."BaseLine" = T2."LineNum"

WHERE 
  T0."CardCode" = 'C1173'
  AND T0."DocDate" >= ADD_DAYS(CURRENT_DATE, -90)
  AND T0."DocDate" >= '20250811'
  AND T0."U_PO_CHUBUT" IS NOT NULL
  AND T0."U_SUB_PO_CHUBUT" IS NOT NULL
  AND (CASE
    WHEN COALESCE(T5."DocEntry", T6."DocEntry") IS NOT NULL OR (T3."DocNum" IS NULL AND T0."DocStatus" = 'C') THEN 7
    WHEN COALESCE(T4."DocEntry", T5."DocEntry") IS NOT NULL THEN 6
    WHEN T3."DocNum" IS NOT NULL AND T3."U_EstadoRemito" = 'DIS' THEN 5
    WHEN T3."DocNum" IS NOT NULL AND T3."U_ESTADO_LOGISTICA" IN (2, 3) THEN 4
    WHEN T3."DocNum" IS NOT NULL AND T3."U_ESTADO_LOGISTICA" = 1 THEN 3
    WHEN T3."DocNum" IS NOT NULL AND T3."U_ESTADO_LOGISTICA" = 0 THEN 2
    WHEN T3."DocNum" IS NULL THEN 1
    ELSE -1 
    END) <> -1

GROUP BY 
  T0."U_PO_CHUBUT",
  T0."U_SUB_PO_CHUBUT",
  T1."ItemCode",
  T3."PTICode",
  T3."Letter",
  T3."FolNumFrom",
  T2."Quantity",
  T3."U_ESTADO_LOGISTICA",
  T3."U_EstadoRemito",
  T4."DocEntry",
  T5."DocEntry",
  T6."DocEntry",
  T3."DocNum",
  T0."DocNum",
  T0."DocStatus",
  T7."U_CodigoInterfaz"

ORDER BY 
  T0."DocNum" DESC;
