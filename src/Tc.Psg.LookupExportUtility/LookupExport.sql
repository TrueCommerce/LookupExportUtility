SELECT
	@pluginId,
	plugin.BSPName,
	CASE luf.LookupType
		WHEN 1 THEN 'Partner'
		WHEN 2 THEN 'Company'
	END AS [LookupType],
	luf.LookupName,
	prt.PartnerName AS [LookupPartner],
	ISNULL(plu.InternalValue, clu.InternalValue) AS [InternalValue],
	ISNULL(plu.ExternalValue, clu.ExternalValue) AS [ExternalValue]

FROM ttcBspPlugIns AS plugin
	JOIN ttcBspLookupFields AS luf
		ON plugin.BspPlugInID = luf.BspPlugInID
	LEFT JOIN ttcPrtLookupData AS plu
		ON luf.BspLookupFieldsID = plu.BspLookupFieldsID
	LEFT JOIN ttcPrtPartnerConfig AS prtcfg
		ON plu.PartnerConfigID = prtcfg.PartnerConfigID
	LEFT JOIN ttcPrtPartners AS prt
		ON prtcfg.PartnerID = prt.PartnerID
	LEFT JOIN ttcBspCompanyLookupData AS clu
		ON luf.BspLookupFieldsID = clu.BspLookupFieldsID

WHERE plugin.BspPlugInID = @pluginId