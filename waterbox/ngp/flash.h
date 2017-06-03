//---------------------------------------------------------------------------
// NEOPOP : Emulator as in Dreamland
//
// Copyright (c) 2001-2002 by neopop_uk
//---------------------------------------------------------------------------

//---------------------------------------------------------------------------
//	This program is free software; you can redistribute it and/or modify
//	it under the terms of the GNU General Public License as published by
//	the Free Software Foundation; either version 2 of the License, or
//	(at your option) any later version. See also the license.txt file for
//	additional informations.
//---------------------------------------------------------------------------

#ifndef __NEOPOP_FLASH__
#define __NEOPOP_FLASH__
//=============================================================================

namespace MDFN_IEN_NGP
{

//Marks flash blocks for saving.
void flash_write(uint32 start_address, uint16 length);

bool FLASH_LoadNV(const uint8* data, uint32 size);
void FLASH_SaveNV(void (*callback)(const uint8* data, uint32 size));
bool FLASH_IsModified();
}

//=============================================================================
#endif
