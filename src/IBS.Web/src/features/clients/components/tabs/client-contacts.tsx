import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Star, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/components/ui/toast';
import { useAddContact, useSetPrimaryContact, useRemoveContact } from '@/hooks/use-clients';
import { addContactSchema, type AddContactFormData } from '@/lib/validations/client';
import type { Contact } from '@/types/api';

/**
 * Props for the ClientContactsTab component.
 */
export interface ClientContactsTabProps {
  clientId: string;
  contacts: Contact[];
}

/**
 * Contacts tab with inline add form and contact management.
 */
export function ClientContactsTab({ clientId, contacts }: ClientContactsTabProps) {
  const [showAddForm, setShowAddForm] = React.useState(false);
  const { t } = useTranslation();
  const { addToast } = useToast();
  const addContact = useAddContact();
  const setPrimaryContact = useSetPrimaryContact();
  const removeContact = useRemoveContact();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddContactFormData>({
    resolver: zodResolver(addContactSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      role: '',
      isPrimary: false,
    },
  });

  const handleAddContact = async (data: AddContactFormData) => {
    try {
      await addContact.mutateAsync({
        clientId,
        data: {
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email || undefined,
          phone: data.phone || undefined,
          title: data.role || undefined,
          isPrimary: data.isPrimary,
        },
      });
      addToast({
        title: t('clients.detail.contacts.addContact'),
        description: t('clients.toast.contactAdded', { name: `${data.firstName} ${data.lastName}` }),
        variant: 'success',
      });
      reset();
      setShowAddForm(false);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.contactAddFailed'),
        variant: 'error',
      });
    }
  };

  const handleSetPrimary = async (contact: Contact) => {
    try {
      await setPrimaryContact.mutateAsync({
        clientId,
        contactId: contact.id,
      });
      addToast({
        title: t('clients.toast.primaryContactUpdated'),
        description: t('clients.toast.primaryContactUpdatedDesc', { name: `${contact.firstName} ${contact.lastName}` }),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.contactUpdateFailed'),
        variant: 'error',
      });
    }
  };

  const handleRemove = async (contact: Contact) => {
    try {
      await removeContact.mutateAsync({
        clientId,
        contactId: contact.id,
      });
      addToast({
        title: t('clients.toast.contactRemoved'),
        description: t('clients.toast.contactRemovedDesc', { name: `${contact.firstName} ${contact.lastName}` }),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.toast.contactRemoveFailed'),
        variant: 'error',
      });
    }
  };

  return (
    <div className="space-y-6">
      {/* Contact Cards */}
      {contacts.length === 0 && !showAddForm ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <p className="text-muted-foreground">{t('clients.detail.contacts.noContacts')}</p>
            <Button
              variant="outline"
              className="mt-4"
              onClick={() => setShowAddForm(true)}
            >
              <Plus className="mr-2 h-4 w-4" />
              {t('clients.detail.contacts.addContact')}
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {contacts.map((contact) => (
            <Card key={contact.id}>
              <CardContent className="pt-6">
                <div className="flex items-start justify-between">
                  <div>
                    <div className="flex items-center gap-2">
                      <h3 className="font-medium">
                        {contact.firstName} {contact.lastName}
                      </h3>
                      {contact.isPrimary && (
                        <Badge variant="secondary" className="text-xs">
                          <Star className="mr-1 h-3 w-3" />
                          {t('clients.detail.contacts.primary')}
                        </Badge>
                      )}
                    </div>
                    {contact.title && (
                      <p className="text-sm text-muted-foreground">{contact.title}</p>
                    )}
                  </div>
                  <div className="flex gap-1">
                    {!contact.isPrimary && (
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8"
                        onClick={() => handleSetPrimary(contact)}
                        title={t('clients.detail.contacts.setPrimary')}
                      >
                        <Star className="h-4 w-4" />
                      </Button>
                    )}
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-destructive hover:text-destructive"
                      onClick={() => handleRemove(contact)}
                      title={t('clients.detail.contacts.remove')}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
                <div className="mt-4 space-y-2 text-sm">
                  {contact.email && (
                    <div>
                      <span className="text-muted-foreground">{t('clients.detail.contacts.email')}: </span>
                      <a href={`mailto:${contact.email}`} className="text-primary hover:underline">
                        {contact.email}
                      </a>
                    </div>
                  )}
                  {contact.phone && (
                    <div>
                      <span className="text-muted-foreground">{t('clients.detail.contacts.phone')}: </span>
                      <a href={`tel:${contact.phone}`} className="hover:underline">
                        {contact.phone}
                      </a>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Add Contact Form */}
      {showAddForm ? (
        <Card>
          <CardHeader>
            <CardTitle>{t('clients.detail.contacts.addContact')}</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(handleAddContact)} className="space-y-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="firstName">{t('clients.detail.contacts.firstName')} *</Label>
                  <Input
                    id="firstName"
                    {...register('firstName')}
                    error={errors.firstName?.message}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">{t('clients.detail.contacts.lastName')} *</Label>
                  <Input
                    id="lastName"
                    {...register('lastName')}
                    error={errors.lastName?.message}
                  />
                </div>
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="email">{t('clients.detail.contacts.email')}</Label>
                  <Input
                    id="email"
                    type="email"
                    {...register('email')}
                    error={errors.email?.message}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phone">{t('clients.detail.contacts.phone')}</Label>
                  <Input
                    id="phone"
                    type="tel"
                    {...register('phone')}
                    error={errors.phone?.message}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="role">{t('clients.detail.contacts.roleTitle')}</Label>
                <Input id="role" {...register('role')} placeholder="e.g., CEO, Owner" />
              </div>
              <div className="flex justify-end gap-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    reset();
                    setShowAddForm(false);
                  }}
                >
                  {t('common.actions.cancel')}
                </Button>
                <Button type="submit" disabled={addContact.isPending}>
                  {addContact.isPending ? t('common.form.creating') : t('clients.detail.contacts.addContact')}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      ) : contacts.length > 0 ? (
        <Button variant="outline" onClick={() => setShowAddForm(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t('clients.detail.contacts.addContact')}
        </Button>
      ) : null}
    </div>
  );
}
