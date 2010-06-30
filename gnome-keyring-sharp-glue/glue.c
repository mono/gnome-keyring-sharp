#include <gnome-keyring.h>

gpointer gks_attribute_list_new ()
{
	return gnome_keyring_attribute_list_new ();
}

gint32 gks_item_attribute_list_get_length (GnomeKeyringAttributeList *attrs)
{
	return (*attrs).len;
}

gboolean gks_item_attribute_list_index_is_string (GnomeKeyringAttributeList *attrs, gint32 index)
{
	return gnome_keyring_attribute_list_index (attrs, index).type == GNOME_KEYRING_ATTRIBUTE_TYPE_STRING;
}

gboolean gks_item_attribute_list_index_is_uint32 (GnomeKeyringAttributeList *attrs, gint32 index)
{
	return gnome_keyring_attribute_list_index (attrs, index).type == GNOME_KEYRING_ATTRIBUTE_TYPE_UINT32;
}

char * gks_item_attribute_list_get_index_string (GnomeKeyringAttributeList *attrs, gint32 index)
{
	return gnome_keyring_attribute_list_index (attrs, index).value.string;
}

guint32 gks_item_attribute_list_get_index_uint32 (GnomeKeyringAttributeList *attrs, gint32 index)
{
	return gnome_keyring_attribute_list_index (attrs, index).value.integer;
}

char * gks_item_attribute_list_get_index_key (GnomeKeyringAttributeList *attrs, gint32 index)
{
	return gnome_keyring_attribute_list_index (attrs, index).name;
}
